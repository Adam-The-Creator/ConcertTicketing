using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[PrimaryKey("ConcertId", "ArtistId", "RoleId")]
public partial class ArtistRolesAtConcert
{
    [Key]
    [Column("ConcertID")]
    public long ConcertId { get; set; }

    [Key]
    [Column("ArtistID")]
    public long ArtistId { get; set; }

    [Key]
    [Column("RoleID")]
    public byte RoleId { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("ArtistRolesAtConcerts")]
    public virtual Artist Artist { get; set; } = null!;

    [ForeignKey("ConcertId")]
    [InverseProperty("ArtistRolesAtConcerts")]
    public virtual Concert Concert { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("ArtistRolesAtConcerts")]
    public virtual ArtistRole Role { get; set; } = null!;
}
