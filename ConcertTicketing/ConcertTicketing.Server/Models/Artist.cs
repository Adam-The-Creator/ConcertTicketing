using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Name", Name = "UQ_Artists_Name", IsUnique = true)]
public partial class Artist
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(128)]
    public string Name { get; set; } = null!;

    [InverseProperty("Artist")]
    public virtual ICollection<ConcertRole> ConcertRoles { get; set; } = new List<ConcertRole>();

    [InverseProperty("MainArtist")]
    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();

    [InverseProperty("Artist")]
    public virtual ICollection<GenresOfArtist> GenresOfArtists { get; set; } = new List<GenresOfArtist>();
}
