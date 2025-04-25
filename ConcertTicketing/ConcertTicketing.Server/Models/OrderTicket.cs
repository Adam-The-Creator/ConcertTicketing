using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("OrderId", Name = "IX_OrderTickets_OrdersID")]
[Index("TicketId", Name = "IX_OrderTickets_TicketID")]
public partial class OrderTicket
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [Column("TicketID")]
    public Guid? TicketId { get; set; }

    [Column("OrderID")]
    public Guid? OrderId { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderTickets")]
    public virtual Order? Order { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("OrderTickets")]
    public virtual Ticket? Ticket { get; set; }
}
