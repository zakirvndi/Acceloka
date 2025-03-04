using MediatR;
using Acceloka.Models.DTOS;
using System.Collections.Generic;

namespace Acceloka.Application.Queries.Tickets
{
    public class GetAvailableTicketsQuery : IRequest<List<TicketDto>>
    {
        public string Category { get; set; }
        public string TicketCode { get; set; }
        public decimal? TicketPrice { get; set; }
        public DateTimeOffset? MinEventDate { get; set; }
        public DateTimeOffset? MaxEventDate { get; set; }
        public string OrderBy { get; set; }
        public string OrderState { get; set; }
    }
}
