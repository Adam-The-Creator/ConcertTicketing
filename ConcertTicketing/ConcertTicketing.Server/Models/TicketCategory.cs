using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("ConcertId", Name = "IX_TicketCategories_ConcertID")]
public partial class TicketCategory
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [StringLength(256)]
    public string? Description { get; set; }

    [Column(TypeName = "money")]
    public decimal? Price { get; set; }

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
    [InverseProperty("TicketCategories")]
    public virtual Concert? Concert { get; set; }

    [InverseProperty("TicketCategory")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
