namespace Acceloka.Models.DTOS
{
    public class BookedTicketGroupedDto
    {
        public string CategoryName { get; set; }
        public decimal SummaryPrice { get; set; }
        public List<BookedTicketDetailDto> Tickets { get; set; }
    }

    public class BookedTicketDetailDto
    {
        public string TicketCode { get; set; }
        public string TicketName { get; set; }
        public decimal Price { get; set; }
    }

    public class BookedTicketResponseDto
    {
        public decimal PriceSummary { get; set; }
        public List<BookedTicketGroupedDto> TicketsPerCategories { get; set; }
    }
}
