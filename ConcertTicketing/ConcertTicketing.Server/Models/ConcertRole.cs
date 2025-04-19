using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[PrimaryKey("ConcertId", "ArtistId", "RoleId")]
public partial class ConcertRole
{
    [Key]
    [Column("ConcertID")]
    public long ConcertId { get; set; }

    [Key]
    [Column("ArtistID")]
    public int ArtistId { get; set; }

    [Key]
    [Column("RoleID")]
    public byte RoleId { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("ConcertRoles")]
    public virtual Artist Artist { get; set; } = null!;

    [ForeignKey("ConcertId")]
    [InverseProperty("ConcertRoles")]
    public virtual Concert Concert { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("ConcertRoles")]
    public virtual Role Role { get; set; } = null!;
}
