namespace Acceloka.Models.DTOS
{
    public class BookedTicketGetResponseDto
    {
        public List<BookedTicketGetGroupedDto> TicketsPerCategories { get; set; }
    }

    public class BookedTicketGetGroupedDto
    {
        public string CategoryName { get; set; }
        public int QtyPerCategory { get; set; }
        public List<BookedTicketGetDetailDto> Tickets { get; set; }
    }

    public class BookedTicketGetDetailDto
    {
        public string TicketCode { get; set; }
        public string TicketName { get; set; }
        public string EventDate { get; set; }
        public int Quantity { get; set; }
    }
}
