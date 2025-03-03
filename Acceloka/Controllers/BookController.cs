using Acceloka.Models.DTOS;
using Acceloka.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acceloka.Controllers
{
    [Route("api/v1/book-ticket")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;

        public BookController(BookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] List<BookTicket> bookingRequest)
        {
            if (bookingRequest == null || bookingRequest.Count == 0)
            {
                return BadRequest(new { message = "Booking request tidak boleh kosong." });
            }

            Log.Information("Request Booking: {@bookingRequest}", bookingRequest);

            
            var result = await _bookService.CreateBookingAsync(bookingRequest);
            return Ok(new { message = "Booking berhasil!", data = result });
        }
    }
}

