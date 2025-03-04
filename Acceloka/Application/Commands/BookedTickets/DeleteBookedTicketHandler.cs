using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using Serilog;

namespace Acceloka.Application.Commands.BookedTickets
{
    public class DeleteBookedTicketHandler : IRequestHandler<DeleteBookedTicketCommand, BookedTicketUpdateDeleteDto>
    {
        private readonly AccelokaContext _db;

        public DeleteBookedTicketHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<BookedTicketUpdateDeleteDto> Handle(DeleteBookedTicketCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var bookedTicket = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .FirstOrDefaultAsync(bt => bt.BookId == request.BookId && bt.TicketCode == request.TicketCode, cancellationToken);

                if (bookedTicket == null)
                {
                    throw new KeyNotFoundException($"Booked ticket dengan BookId {request.BookId} dan kode {request.TicketCode} tidak ditemukan.");
                }

                if (request.Quantity > bookedTicket.Quantity)
                {
                    throw new ArgumentException($"Qty yang ingin direvoke ({request.Quantity}) melebihi jumlah tiket yang dipesan ({bookedTicket.Quantity}).");
                }

                // Ini mengembalikan data quota ke table ticket setelah tiket dibatalkan/direvoke
                var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketCode == request.TicketCode, cancellationToken);
                if (ticket != null)
                {
                    ticket.Quota += request.Quantity;
                }

                // Jika Qty == jumlah yang dipesan, hapus BookedTicket
                if (request.Quantity == bookedTicket.Quantity)
                {
                    _db.BookedTickets.Remove(bookedTicket);
                }
                else
                {
                    bookedTicket.Quantity -= request.Quantity;
                }

                await _db.SaveChangesAsync(cancellationToken);

                // Ambil ulang daftar tiket dalam BookId ini setelah perubahan
                var remainingTickets = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .Where(bt => bt.BookId == request.BookId)
                    .ToListAsync(cancellationToken);

                // Jika semua tiket dalam BookId ini sudah dihapus, hapus juga entry di tabel Book
                if (!remainingTickets.Any())
                {
                    var book = await _db.Books.FindAsync(request.BookId);
                    if (book != null)
                    {
                        _db.Books.Remove(book);
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);

                Log.Information("Revoke ticket berhasil: {TicketCode} dengan qty {Qty}", request.TicketCode, request.Quantity);

                return new BookedTicketUpdateDeleteDto
                {
                    BookId = request.BookId,
                    RemainingTickets = remainingTickets.Select(rt => new BookedTicketUpdateDeleteGroupedDto
                    {
                        CategoryName = rt.TicketCodeNavigation.Category,
                        SummaryPrice = rt.TicketCodeNavigation.TicketPrice * rt.Quantity,
                        Tickets = new List<BookedTicketUpdateDeleteDetailDto>
                        {
                            new BookedTicketUpdateDeleteDetailDto
                            {
                                TicketCode = rt.TicketCode,
                                TicketName = rt.TicketCodeNavigation.TicketName,
                                Quantity = rt.Quantity
                            }
                        }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                Log.Error(ex, "Error saat melakukan revoke tiket.");
                throw;
            }
        }
    }
}
