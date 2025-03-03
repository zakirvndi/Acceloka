using Acceloka.Models.DTOS;
using Acceloka.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acceloka.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class BookedTicketController : ControllerBase
    {
        private readonly BookedTicketService _bookedTicketService;

        public BookedTicketController(BookedTicketService bookedTicketService)
        {
            _bookedTicketService = bookedTicketService;
        }

        // GET: api/<BookedTicketController>
        [HttpGet("get-booked-ticket/{bookId}")]
        public async Task<IActionResult> GetBookedTicket(int bookId)
        {
            var result = await _bookedTicketService.GetBookedTicketByIdAsync(bookId);
            return Ok(result); 
        }

        [HttpDelete("revoke-ticket/{bookId}/{ticketCode}/{qty}")]
        public async Task<IActionResult> RevokeTicket(int bookId, string ticketCode, int qty)
        {
            var result = await _bookedTicketService.RevokeBookedTicketAsync(bookId, ticketCode, qty);
            return Ok(new
            {
                message = "Revoke tiket berhasil",
                data = result
            });
        }

        [HttpPut("update-ticket/{bookId}")]
        public async Task<IActionResult> UpdateTicket(int bookId, [FromBody] List<BookedTicketUpdateRequestDto> updatedTickets)
        {
            var result = await _bookedTicketService.UpdateBookedTicketAsync(bookId, updatedTickets);
            return Ok(new
            {
                message = "Update tiket berhasil",
                data = result
            });
        }
    }
}
