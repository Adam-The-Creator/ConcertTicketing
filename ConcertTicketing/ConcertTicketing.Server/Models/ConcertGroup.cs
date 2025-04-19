using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

public partial class ConcertGroup
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string? Name { get; set; }

    [InverseProperty("ConcertGroup")]
    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();
}
