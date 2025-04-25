using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("ConcertId", Name = "IX_TicketDetails_ConcertID")]
public partial class TicketDetail
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [StringLength(256)]
    public string? Description { get; set; }

    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string? Area { get; set; }

    [Column("ConcertID")]
    public long? ConcertId { get; set; }

    [ForeignKey("ConcertId")]
    [InverseProperty("TicketDetails")]
    public virtual Concert? Concert { get; set; }

    [InverseProperty("TicketDetail")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
