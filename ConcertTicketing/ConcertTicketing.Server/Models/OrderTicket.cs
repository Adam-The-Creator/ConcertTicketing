using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

public partial class OrderTicket
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TicketID")]
    public Guid? TicketId { get; set; }

    [Column("OrdersID")]
    public Guid? OrdersId { get; set; }

    [ForeignKey("OrdersId")]
    [InverseProperty("OrderTickets")]
    public virtual Order? Orders { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("OrderTickets")]
    public virtual Ticket? Ticket { get; set; }
}
