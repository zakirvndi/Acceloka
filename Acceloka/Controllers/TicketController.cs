using Acceloka.Models.DTOS;
using Acceloka.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acceloka.Controllers
{
    [Route("api/v1/get-available-ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAvailableTickets([FromQuery] TicketDto filter)
        {
            var tickets = await _ticketService.GetAvailableTickets(filter);
            return Ok(new { success = true, data = tickets });
        }
    }
}
