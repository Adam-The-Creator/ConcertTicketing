using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("ArtistName", Name = "IX_Artists_ArtistName")]
[Index("ArtistName", Name = "UQ_Artists_Name", IsUnique = true)]
public partial class Artist
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(128)]
    public string ArtistName { get; set; } = null!;

    [InverseProperty("Artist")]
    public virtual ICollection<ArtistRolesAtConcert> ArtistRolesAtConcerts { get; set; } = new List<ArtistRolesAtConcert>();

    [InverseProperty("MainArtist")]
    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();

    [InverseProperty("Artist")]
    public virtual ICollection<GenresOfArtist> GenresOfArtists { get; set; } = new List<GenresOfArtist>();
}
