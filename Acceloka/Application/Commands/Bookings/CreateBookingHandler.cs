using System.Transactions;
using Acceloka.Application.Commands.Bookings;
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookedTicketResponseDto>
{
    private readonly AccelokaContext _db;

    public CreateBookingHandler(AccelokaContext db)
    {
        _db = db;
    }

    public async Task<BookedTicketResponseDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var totalQuantity = request.Tickets.Sum(bt => bt.Quantity);
            var bookedTickets = new List<BookedTicket>();

            // Ambil semua tiket yang diminta
            var ticketCodes = request.Tickets.Select(bt => bt.TicketCode).ToList();
            var tickets = await _db.Tickets.Where(t => ticketCodes.Contains(t.TicketCode)).ToListAsync(cancellationToken);

            // Konversi list tiket ke dictionary untuk optimasi lookup
            var availableTickets = tickets.ToDictionary(t => t.TicketCode);

            foreach (var ticketDto in request.Tickets)
            {
                if (string.IsNullOrEmpty(ticketDto.TicketCode) || ticketDto.Quantity <= 0)
                {
                    throw new ArgumentException($"Tiket dengan kode {ticketDto.TicketCode} tidak valid.");
                }

                if (!availableTickets.TryGetValue(ticketDto.TicketCode, out var ticket) || ticket.Quota < ticketDto.Quantity)
                {
                    throw new Exception($"Tiket dengan kode {ticketDto.TicketCode} tidak tersedia atau stok tidak mencukupi.");
                }

                bookedTickets.Add(new BookedTicket
                {
                    TicketCode = ticketDto.TicketCode,
                    Quantity = ticketDto.Quantity
                });

                Log.Information("Mengurangi quota tiket: {TicketCode}, Jumlah: {Quantity}, Sisa sebelum dikurangi: {Quota}",
                    ticket.TicketCode, ticketDto.Quantity, ticket.Quota);

                ticket.Quota -= ticketDto.Quantity;
            }

            await _db.SaveChangesAsync(cancellationToken);

            var newBook = new Book
            {
                BookingDate = DateTimeOffset.UtcNow,
                TotalQuantity = totalQuantity,
                TotalPrice = bookedTickets.Sum(bt => availableTickets[bt.TicketCode].TicketPrice * bt.Quantity),
                BookedTickets = bookedTickets
            };

            _db.Books.Add(newBook);
            await _db.SaveChangesAsync(cancellationToken);

            transaction.Complete();

            Log.Information("Booking berhasil dengan ID {BookId} dan Total Price {TotalPrice}", newBook.BookId, newBook.TotalPrice);

            return GenerateGroupedBookedTicketResponse(newBook, availableTickets);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saat melakukan booking ticket. Semua perubahan dibatalkan.");
            throw;
        }
    }

    private BookedTicketResponseDto GenerateGroupedBookedTicketResponse(Book book, Dictionary<string, Ticket> availableTickets)
    {
        try
        {
            var groupedTickets = book.BookedTickets
                .GroupBy(bt => availableTickets[bt.TicketCode].Category)
                .Select(g => new BookedTicketGroupedDto
                {
                    CategoryName = g.Key,
                    SummaryPrice = g.Sum(bt => availableTickets[bt.TicketCode].TicketPrice * bt.Quantity),
                    Tickets = g.Select(bt => new BookedTicketDetailDto
                    {
                        TicketCode = bt.TicketCode,
                        TicketName = availableTickets[bt.TicketCode].TicketName,
                        Price = availableTickets[bt.TicketCode].TicketPrice
                    }).ToList()
                }).ToList();

            return new BookedTicketResponseDto
            {
                PriceSummary = groupedTickets.Sum(g => g.SummaryPrice),
                TicketsPerCategories = groupedTickets
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saat mengenerate response booking.");
            throw;
        }
    }
}
