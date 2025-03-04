using Acceloka.Application.Commands.Bookings;
using MediatR;
using Microsoft.AspNetCore.Mvc;



namespace Acceloka.Controllers
{
    [Route("api/v1/book-ticket")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

