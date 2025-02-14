using System;
using System.Text.Json.Serialization;

namespace Acceloka.Models.DTOS
{
    public class TicketDto
    {
        public DateTimeOffset EventDate { get; set; }
        public int Quota { get; set; }
        public string? Category { get; set; }
        public string? TicketCode { get; set; }
        public string? TicketName { get; set; }
        public decimal? TicketPrice { get; set; }

        [JsonIgnore]
        public DateTimeOffset? MinEventDate { get; set; }
        [JsonIgnore]
        public DateTimeOffset? MaxEventDate { get; set; }
        [JsonIgnore]
        public string OrderBy { get; set; } = "TicketCode";
        [JsonIgnore]
        public string OrderState { get; set; } = "ascending";
   
       
    }
}
