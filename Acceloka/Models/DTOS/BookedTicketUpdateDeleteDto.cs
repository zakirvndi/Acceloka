namespace Acceloka.Models.DTOS
{
    public class BookedTicketUpdateDeleteDto
    {
        public int BookId { get; set; }  
        public List<BookedTicketUpdateDeleteGroupedDto> RemainingTickets { get; set; } = new List<BookedTicketUpdateDeleteGroupedDto>();
    }

    public class BookedTicketUpdateDeleteGroupedDto
    {
        public string CategoryName { get; set; }
        public decimal SummaryPrice { get; set; }
        public List<BookedTicketUpdateDeleteDetailDto> Tickets { get; set; }
    }

    public class BookedTicketUpdateDeleteDetailDto
    {
        public string TicketCode { get; set; }
        public string TicketName { get; set; }
        public int Quantity { get; set; }  
    }

    public class BookedTicketUpdateRequestDto
    {
        public string TicketCode { get; set; }
        public int Quantity { get; set; }
    }

}
