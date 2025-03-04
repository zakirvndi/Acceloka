using Acceloka.Entities;
using Acceloka.Models;
using Acceloka.Models.DTOS;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Acceloka.Services
{
    public class TicketService
    {
        private readonly AccelokaContext _db;
        private readonly IMapper _mapper;

        public TicketService(AccelokaContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<TicketDto>> GetAvailableTickets(TicketDto filter)
        {
            try
            {
                var query = _db.Tickets.AsQueryable();

                // Filtering jika ada input
                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(Q  => Q.Category.Contains(filter.Category));

                if (!string.IsNullOrEmpty(filter.TicketCode))
                    query = query.Where(Q  => Q.TicketCode.Contains(filter.TicketCode));

                if (filter.TicketPrice.HasValue)
                    query = query.Where(Q  => Q.TicketPrice <= filter.TicketPrice.Value);

                if (filter.MinEventDate.HasValue)
                    query = query.Where(Q  => Q.EventDate >= filter.MinEventDate.Value);

                if (filter.MaxEventDate.HasValue)
                    query = query.Where(Q  => Q.EventDate <= filter.MaxEventDate.Value);

                // Sorting
                query = filter.OrderBy switch
                {
                    "TicketName" => filter.OrderState == "descending" ? query.OrderByDescending(Q  => Q.TicketName) : query.OrderBy(Q  => Q.TicketName),
                    "Category" => filter.OrderState == "descending" ? query.OrderByDescending(Q  => Q.Category) : query.OrderBy(Q  => Q.Category),
                    "TicketPrice" => filter.OrderState == "descending" ? query.OrderByDescending(Q  => Q.TicketPrice) : query.OrderBy(Q  => Q.TicketPrice),
                    "EventDate" => filter.OrderState == "descending" ? query.OrderByDescending(Q  => Q.EventDate) : query.OrderBy(Q  => Q.EventDate),
                    _ => filter.OrderState == "descending" ? query.OrderByDescending(Q  => Q.TicketCode) : query.OrderBy(Q  => Q.TicketCode),
                };

                var ticketList = await query.ToListAsync();

                if (!ticketList.Any())
                {
                    throw new KeyNotFoundException("No tickets found matching the criteria.");
                }

                return _mapper.Map<List<TicketDto>>(ticketList);
            }
            catch (Exception ex)
            {
                //test
                Log.Error(ex, "Error in GetAvailableTickets");
                throw;
            }
        }
    }
}
