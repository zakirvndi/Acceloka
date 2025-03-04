using Acceloka.Models.DTOS;
using MediatR;

public class GetBookedTicketByIdQuery : IRequest<BookedTicketGetResponseDto>
{
    public int BookId { get; set; }

    public GetBookedTicketByIdQuery(int bookId)
    {
        BookId = bookId;
    }
}
