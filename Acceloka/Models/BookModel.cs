namespace Acceloka.Models
{
    public class BookModel
    {
        public int BookId { get; set; }

        public DateTimeOffset? BookingDate { get; set; }

        public decimal TotalPrice { get; set; }

        public int TotalQuantity { get; set; }
    }
}
