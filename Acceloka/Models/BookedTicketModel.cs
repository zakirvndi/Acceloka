namespace Acceloka.Models
{
    public class BookedTicketModel
    {
        public int BookedTicketId { get; set; }
        public int BookingId { get; set; }
        public string TicketCode { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}

