using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using Serilog;

namespace Acceloka.Application.Commands.BookedTickets
{
    public class UpdateBookedTicketHandler : IRequestHandler<UpdateBookedTicketCommand, BookedTicketUpdateDeleteDto>
    {
        private readonly AccelokaContext _db;

        public UpdateBookedTicketHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<BookedTicketUpdateDeleteDto> Handle(UpdateBookedTicketCommand request, CancellationToken cancellationToken)
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

                var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketCode == request.TicketCode, cancellationToken);
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Tiket dengan kode {request.TicketCode} tidak ditemukan.");
                }

                int totalAvailableQuota = ticket.Quota + bookedTicket.Quantity; 
                if (request.Quantity > totalAvailableQuota)
                {
                    throw new ArgumentException($"Quantity yang diminta ({request.Quantity}) melebihi total quota tiket yang tersedia ({totalAvailableQuota}).");
                }

                // Hitung perubahan stok
                int difference = request.Quantity - bookedTicket.Quantity;
                ticket.Quota -= difference; // Jika naik, stok berkurang; jika turun, stok bertambah (di table ticket)
                bookedTicket.Quantity = request.Quantity;

                await _db.SaveChangesAsync(cancellationToken);

                // Ambil ulang daftar tiket dalam BookId ini setelah perubahan
                var remainingTickets = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .Where(bt => bt.BookId == request.BookId)
                    .ToListAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                Log.Information("Update tiket berhasil: {TicketCode} dengan qty {Qty}", request.TicketCode, request.Quantity);

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
                Log.Error(ex, "Error saat melakukan update tiket.");
                throw;
            }
        }
    }
}
