using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Location", Name = "IX_Venues_Location")]
public partial class Venue
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string Location { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string? Type { get; set; }

    public int? Capacity { get; set; }

    [InverseProperty("Venue")]
    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();
}
