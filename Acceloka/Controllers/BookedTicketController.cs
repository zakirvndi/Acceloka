using Acceloka.Application.Commands.BookedTickets;
using Acceloka.Models.DTOS;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Acceloka.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class BookedTicketController : ControllerBase
    {
        //MediatR
        private readonly IMediator _mediator;

        public BookedTicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET
        [HttpGet("get-booked-ticket/{bookId}")]
        public async Task<IActionResult> GetBookedTicketById([FromRoute] int bookId)
        {
            var result = await _mediator.Send(new GetBookedTicketByIdQuery(bookId));
            return Ok(result);
        }

        //DELETE
        [HttpDelete("revoke-ticket/{bookId}/{ticketCode}/{qty}")]
        public async Task<IActionResult> RevokeTicket(int bookId, string ticketCode, int qty)
        {
            var command = new DeleteBookedTicketCommand(bookId, ticketCode, qty);
            var result = await _mediator.Send(command);
            return Ok(new
            {
                message = "Revoke tiket berhasil",
                data = result
            });
        }

        [HttpPut("update-ticket/{bookId}")]
        public async Task<IActionResult> UpdateBookedTicket(int bookId, [FromBody] UpdateBookedTicketCommand command)
        {
            command.BookId = bookId; // Pastikan BookId diambil dari URL
            var result = await _mediator.Send(command);
            return Ok(new
            {
                message = "Update tiket berhasil",
                data = result
            });
        }
    }
}
