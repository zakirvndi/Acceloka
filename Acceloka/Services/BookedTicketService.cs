using Acceloka.Entities;
using Acceloka.Models.DTOS;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;

namespace Acceloka.Services
{
    public class BookedTicketService
    {
        private readonly AccelokaContext _db;
        private readonly IMapper _mapper;

        public BookedTicketService(AccelokaContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        //untuk disini sebenarnya menggunakan BookId bukan BookedTicketId untuk parameter GET melihat tiket yang sudah dibooking, karena relasi di databasenya yang saya buat
        //table Book ke BookedTicket 1 to Many, dimana 1 kali booking bisa memiliki banyak tiket (BookedTicket). Sehingga ketika mau melihat Booking tertentu memesan tiket apa saja, bisa dilihat berdasarkan BookIdnya
        public async Task<BookedTicketGetResponseDto> GetBookedTicketByIdAsync(int bookedTicketId)
        {
            var bookedTickets = await _db.BookedTickets
                .Where(bt => bt.BookId == bookedTicketId)
                .Include(bt => bt.TicketCodeNavigation)
                .ToListAsync();

            if (!bookedTickets.Any())
            {
                throw new KeyNotFoundException($"BookedTicket with ID {bookedTicketId} not found.");
            }

            // Group by Category
            var groupedTickets = bookedTickets
                .GroupBy(bt => bt.TicketCodeNavigation.Category)
                .Select(group => new BookedTicketGetGroupedDto
                {
                    CategoryName = group.Key,
                    QtyPerCategory = group.Sum(bt => bt.Quantity),
                    Tickets = _mapper.Map<List<BookedTicketGetDetailDto>>(group.ToList())
                })
                .ToList();

            return new BookedTicketGetResponseDto
            {
                TicketsPerCategories = groupedTickets
            };
        }

        public async Task<BookedTicketUpdateDeleteDto> RevokeBookedTicketAsync(int bookId, string ticketCode, int qty)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var bookedTicket = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .FirstOrDefaultAsync(bt => bt.BookId == bookId && bt.TicketCode == ticketCode);

                if (bookedTicket == null)
                {
                    throw new KeyNotFoundException($"Booked ticket dengan BookId {bookId} dan kode {ticketCode} tidak ditemukan.");
                }

                if (qty > bookedTicket.Quantity)
                {
                    throw new ArgumentException($"Qty yang ingin direvoke ({qty}) melebihi jumlah tiket yang dipesan ({bookedTicket.Quantity}).");
                }

                // Kembalikan kuota tiket ke tabel Ticket
                var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
                if (ticket != null)
                {
                    ticket.Quota += qty;
                }

                // Jika Qty == jumlah yang dipesan, hapus BookedTicket
                if (qty == bookedTicket.Quantity)
                {
                    _db.BookedTickets.Remove(bookedTicket);
                }
                else
                {
                    bookedTicket.Quantity -= qty;
                }

                await _db.SaveChangesAsync();

                // Ambil ulang daftar tiket dalam BookId ini setelah perubahan
                var remainingTickets = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .Where(bt => bt.BookId == bookId)
                    .ToListAsync();

                // Jika semua tiket dalam BookId ini sudah dihapus, hapus juga entry di tabel Book
                if (!remainingTickets.Any())
                {
                    var book = await _db.Books.FindAsync(bookId);
                    if (book != null)
                    {
                        _db.Books.Remove(book);
                        await _db.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                Log.Information("Revoke ticket berhasil: {TicketCode} dengan qty {Qty}", ticketCode, qty);

                return new BookedTicketUpdateDeleteDto
                {
                    BookId = bookId,
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
                await transaction.RollbackAsync();
                Log.Error(ex, "Error saat melakukan revoke tiket.");
                throw;
            }
        }


        public async Task<BookedTicketUpdateDeleteDto> UpdateBookedTicketAsync(int bookId, List<BookedTicketUpdateRequestDto> updatedTickets)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var ticketCodes = updatedTickets.Select(ut => ut.TicketCode).ToList();
                var bookedTickets = await _db.BookedTickets
                    .Include(bt => bt.TicketCodeNavigation)
                    .Where(bt => bt.BookId == bookId && ticketCodes.Contains(bt.TicketCode))
                    .ToListAsync();

                if (!bookedTickets.Any())
                {
                    throw new KeyNotFoundException($"Tidak ada tiket yang ditemukan untuk BookId {bookId}.");
                }

                var tickets = await _db.Tickets.Where(t => ticketCodes.Contains(t.TicketCode)).ToListAsync();

                foreach (var updatedTicket in updatedTickets)
                {
                    var bookedTicket = bookedTickets.FirstOrDefault(bt => bt.TicketCode == updatedTicket.TicketCode);
                    if (bookedTicket == null)
                    {
                        throw new KeyNotFoundException($"Tiket dengan kode {updatedTicket.TicketCode} tidak ditemukan dalam booking.");
                    }

                    var ticket = tickets.FirstOrDefault(t => t.TicketCode == updatedTicket.TicketCode);
                    if (ticket == null)
                    {
                        throw new KeyNotFoundException($"Tiket dengan kode {updatedTicket.TicketCode} tidak ditemukan.");
                    }

                    if (updatedTicket.Quantity < 1)
                    {
                        throw new ArgumentException($"Quantity untuk tiket {updatedTicket.TicketCode} minimal harus 1. Gunakan API DELETE jika ingin menghapus.");
                    }

                    if (updatedTicket.Quantity > (ticket.Quota + bookedTicket.Quantity))
                    {
                        throw new ArgumentException($"Quantity yang diminta ({updatedTicket.Quantity}) melebihi total quota tiket yang tersedia ({ticket.Quota + bookedTicket.Quantity}).");
                    }

                    // ✅ Update jumlah tiket yang sudah dipesan
                    int delta = updatedTicket.Quantity - bookedTicket.Quantity;
                    ticket.Quota -= delta;
                    bookedTicket.Quantity = updatedTicket.Quantity;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.Information("Update ticket berhasil untuk BookId {BookId}", bookId);

                // ✅ Mapping secara manual (tanpa AutoMapper)
                var groupedRemainingTickets = bookedTickets
                    .GroupBy(bt => bt.TicketCodeNavigation.Category)
                    .Select(g => new BookedTicketUpdateDeleteGroupedDto
                    {
                        CategoryName = g.Key,
                        SummaryPrice = g.Sum(bt => bt.TicketCodeNavigation.TicketPrice * bt.Quantity),
                        Tickets = g.Select(bt => new BookedTicketUpdateDeleteDetailDto
                        {
                            TicketCode = bt.TicketCode,
                            TicketName = bt.TicketCodeNavigation.TicketName,
                            Quantity = bt.Quantity
                        }).ToList()
                    }).ToList();

                return new BookedTicketUpdateDeleteDto
                {
                    BookId = bookId,
                    RemainingTickets = groupedRemainingTickets
                };
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error saat melakukan update tiket.");
                throw;
            }
        }


    }
}
