using MediatR;
using Acceloka.Models.DTOS;

namespace Acceloka.Application.Commands.Bookings
{
    public class CreateBookingCommand : IRequest<BookedTicketResponseDto>
    {
        public List<BookTicketDto> Tickets { get; set; } = new List<BookTicketDto>();
    }
}
