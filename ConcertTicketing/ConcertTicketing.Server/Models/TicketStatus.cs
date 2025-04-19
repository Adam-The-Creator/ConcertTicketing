using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Status", Name = "UQ_TicketStatuses_Status", IsUnique = true)]
public partial class TicketStatus
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(32)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [InverseProperty("TicketStatus")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
