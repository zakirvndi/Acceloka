using Acceloka.Models.DTOS;
using AutoMapper;
using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Entities;

public class GetBookedTicketByIdHandler : IRequestHandler<GetBookedTicketByIdQuery, BookedTicketGetResponseDto>
{
    private readonly AccelokaContext _db;
    private readonly IMapper _mapper;

    public GetBookedTicketByIdHandler(AccelokaContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<BookedTicketGetResponseDto> Handle(GetBookedTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var bookedTickets = await _db.BookedTickets
            .Where(bt => bt.BookId == request.BookId)
            .Include(bt => bt.TicketCodeNavigation)
            .ToListAsync(cancellationToken);

        if (!bookedTickets.Any())
        {
            throw new KeyNotFoundException($"BookedTicket with ID {request.BookId} not found.");
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
}
