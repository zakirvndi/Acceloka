﻿using MediatR;
using Acceloka.Models.DTOS;

namespace Acceloka.Application.Commands.BookedTickets
{
    public class DeleteBookedTicketCommand : IRequest<BookedTicketUpdateDeleteDto>
    {
        public int BookId { get; set; }
        public string TicketCode { get; set; }
        public int Quantity { get; set; }

        public DeleteBookedTicketCommand(int bookId, string ticketCode, int quantity)
        {
            BookId = bookId;
            TicketCode = ticketCode;
            Quantity = quantity;
        }
    }
}
