using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("ConcertId", Name = "IX_Tickets_ConcertID")]
[Index("SerialNumber", Name = "UQ_Tickets_SerialNumber", IsUnique = true)]
public partial class Ticket
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string SerialNumber { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string? Seat { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PurchaseDate { get; set; }

    [Column("TicketCategoryID")]
    public long? TicketCategoryId { get; set; }

    [Column("ConcertID")]
    public long? ConcertId { get; set; }

    [Column("TicketStatusID")]
    public byte? TicketStatusId { get; set; }

    [ForeignKey("ConcertId")]
    [InverseProperty("Tickets")]
    public virtual Concert? Concert { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();

    [ForeignKey("TicketCategoryId")]
    [InverseProperty("Tickets")]
    public virtual TicketCategory? TicketCategory { get; set; }

    [ForeignKey("TicketStatusId")]
    [InverseProperty("Tickets")]
    public virtual TicketStatus? TicketStatus { get; set; }
}
