using FluentValidation;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Application.Queries.Tickets;
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using Serilog;

public class GetAvailableTicketsHandler : IRequestHandler<GetAvailableTicketsQuery, List<TicketDto>>
{
    private readonly AccelokaContext _db;
    private readonly IMapper _mapper;

    public GetAvailableTicketsHandler(AccelokaContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<TicketDto>> Handle(GetAvailableTicketsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _db.Tickets.AsQueryable();

            // Filtering jika ada input
            if (!string.IsNullOrEmpty(request.Category))
                query = query.Where(t => t.Category.Contains(request.Category));

            if (!string.IsNullOrEmpty(request.TicketCode))
                query = query.Where(t => t.TicketCode.Contains(request.TicketCode));

            if (request.TicketPrice.HasValue)
                query = query.Where(t => t.TicketPrice <= request.TicketPrice.Value);

            if (request.MinEventDate.HasValue)
                query = query.Where(t => t.EventDate >= request.MinEventDate.Value);

            if (request.MaxEventDate.HasValue)
                query = query.Where(t => t.EventDate <= request.MaxEventDate.Value);

            // Sorting
            query = request.OrderBy switch
            {
                "TicketName" => request.OrderState == "descending" ? query.OrderByDescending(t => t.TicketName) : query.OrderBy(t => t.TicketName),
                "Category" => request.OrderState == "descending" ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
                "TicketPrice" => request.OrderState == "descending" ? query.OrderByDescending(t => t.TicketPrice) : query.OrderBy(t => t.TicketPrice),
                "EventDate" => request.OrderState == "descending" ? query.OrderByDescending(t => t.EventDate) : query.OrderBy(t => t.EventDate),
                _ => request.OrderState == "descending" ? query.OrderByDescending(t => t.TicketCode) : query.OrderBy(t => t.TicketCode),
            };

            var ticketList = await query.ToListAsync(cancellationToken);

            if (!ticketList.Any())
            {
                throw new KeyNotFoundException("No tickets found matching the criteria.");
            }

            return _mapper.Map<List<TicketDto>>(ticketList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetAvailableTickets");
            throw;
        }

       
    }
}
