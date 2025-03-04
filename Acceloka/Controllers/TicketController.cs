using Acceloka.Application.Queries.Tickets;
using Acceloka.Application.Validators.Tickets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Acceloka.Controllers
{
    [Route("api/v1/get-available-ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTickets(
            [FromQuery] string? category,
            [FromQuery] string? ticketCode,
            [FromQuery] decimal? ticketPrice,
            [FromQuery] DateTimeOffset? minEventDate,
            [FromQuery] DateTimeOffset? maxEventDate,
            [FromQuery] string? orderBy,
            [FromQuery] string? orderState)
        {
            var query = new GetAvailableTicketsQuery
            {
                Category = category,
                TicketCode = ticketCode,
                TicketPrice = ticketPrice,
                MinEventDate = minEventDate,
                MaxEventDate = maxEventDate,
                OrderBy = orderBy,
                OrderState = orderState
            };

            var tickets = await _mediator.Send(query);
            return Ok(tickets);
        }

    }
}
