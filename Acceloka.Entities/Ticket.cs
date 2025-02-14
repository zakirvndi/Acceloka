using System;
using System.Collections.Generic;

namespace Acceloka.Entities;

public partial class Ticket
{
    public string TicketCode { get; set; } = null!;

    public string TicketName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public int Quota { get; set; }

    public DateTimeOffset EventDate { get; set; }

    public decimal TicketPrice { get; set; }

    public virtual ICollection<BookedTicket> BookedTickets { get; set; } = new List<BookedTicket>();
}
