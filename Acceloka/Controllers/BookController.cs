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
<<<<<<< HEAD
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
=======
        public async Task<IActionResult> CreateBooking([FromBody] List<BookTicketDto> bookingRequest)
>>>>>>> 017008d (Update)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

