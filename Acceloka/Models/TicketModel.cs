using System.ComponentModel.DataAnnotations;

namespace Acceloka.Models
{
    public class TicketModel
    {
        
        public string TicketCode { get; set; }

        public string TicketName { get; set; }

        public string Category { get; set; }

       
        [Range(1, int.MaxValue, ErrorMessage = "Quota must be at least 1.")]
        public int Quota { get; set; }

        
        public DateTimeOffset EventDate { get; set; }

        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal TicketPrice { get; set; }
    }
}
