using System;
using System.Collections.Generic;

namespace Acceloka.Entities;

public partial class Book
{
    public int BookId { get; set; }

    public DateTimeOffset? BookingDate { get; set; }

    public decimal TotalPrice { get; set; }

    public int TotalQuantity { get; set; }

    public virtual ICollection<BookedTicket> BookedTickets { get; set; } = new List<BookedTicket>();
}
