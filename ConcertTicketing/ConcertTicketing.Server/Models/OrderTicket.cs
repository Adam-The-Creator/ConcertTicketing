using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[PrimaryKey("TicketId", "OrderId")]
[Index("OrderId", Name = "IX_OrderTickets_OrdersID")]
[Index("TicketId", Name = "IX_OrderTickets_TicketID")]
[Index("TicketId", Name = "UQ_OrderTickets_TicketID", IsUnique = true)]
public partial class OrderTicket
{
    [Key]
    [Column("TicketID")]
    public Guid TicketId { get; set; }

    [Key]
    [Column("OrderID")]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderTickets")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("TicketId")]
    [InverseProperty("OrderTicket")]
    public virtual Ticket Ticket { get; set; } = null!;
}
