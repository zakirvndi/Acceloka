
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;

using System.Transactions;

namespace Acceloka.Services
{
    public class BookService
    {
        private readonly AccelokaContext _db;

        public BookService(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<BookedTicketResponseDto> CreateBookingAsync(List<BookedTicketDto> bookedTicketsDto)
        {
            if (bookedTicketsDto == null || !bookedTicketsDto.Any())
            {
                throw new ArgumentException("Booking request tidak boleh kosong.");
            }

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var totalQuantity = bookedTicketsDto.Sum(bt => bt.Quantity);
                var bookedTickets = new List<BookedTicket>();

                var ticketCodes = bookedTicketsDto.Select(bt => bt.TicketCode).ToList();
                var tickets = await _db.Tickets.Where(t => ticketCodes.Contains(t.TicketCode)).ToListAsync();

                foreach (var ticketDto in bookedTicketsDto)
                {
                    if (string.IsNullOrEmpty(ticketDto.TicketCode) || ticketDto.Quantity <= 0)
                    {
                        throw new ArgumentException($"Tiket dengan kode {ticketDto.TicketCode} tidak valid.");
                    }

                    var ticket = tickets.FirstOrDefault(t => t.TicketCode == ticketDto.TicketCode);
                    if (ticket == null || ticket.Quota < ticketDto.Quantity)
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

                await _db.SaveChangesAsync();

                var book = new Book
                {
                    BookingDate = DateTimeOffset.UtcNow,
                    TotalQuantity = totalQuantity,
                    TotalPrice = bookedTickets.Sum(bt => tickets.First(t => t.TicketCode == bt.TicketCode).TicketPrice * bt.Quantity),
                    BookedTickets = bookedTickets
                };

                _db.Books.Add(book);
                await _db.SaveChangesAsync();

                transaction.Complete();

                Log.Information("Booking berhasil dengan ID {BookId} dan Total Price {TotalPrice}", book.BookId, book.TotalPrice);

                return GenerateGroupedBookedTicketResponse(book, tickets);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saat melakukan booking ticket. Semua perubahan dibatalkan.");
                throw;
            }
        }

        private BookedTicketResponseDto GenerateGroupedBookedTicketResponse(Book book, List<Ticket> tickets)
        {
            try
            {
                var groupedTickets = book.BookedTickets
                    .GroupBy(bt => tickets.First(t => t.TicketCode == bt.TicketCode).Category)
                    .Select(g => new BookedTicketGroupedDto
                    {
                        CategoryName = g.Key,
                        SummaryPrice = g.Sum(bt => tickets.First(t => t.TicketCode == bt.TicketCode).TicketPrice * bt.Quantity),
                        Tickets = g.Select(bt => new BookedTicketDetailDto
                        {
                            TicketCode = bt.TicketCode,
                            TicketName = tickets.First(t => t.TicketCode == bt.TicketCode).TicketName,
                            Price = tickets.First(t => t.TicketCode == bt.TicketCode).TicketPrice
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
}





