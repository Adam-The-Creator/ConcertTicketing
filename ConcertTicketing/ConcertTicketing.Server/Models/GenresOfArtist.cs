using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("ArtistId", Name = "IX_GenresOfArtists_ArtistID")]
[Index("GenreId", Name = "IX_GenresOfArtists_GenreID")]
public partial class GenresOfArtist
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ArtistID")]
    public int? ArtistId { get; set; }

    [Column("GenreID")]
    public int? GenreId { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("GenresOfArtists")]
    public virtual Artist? Artist { get; set; }

    [ForeignKey("GenreId")]
    [InverseProperty("GenresOfArtists")]
    public virtual Genre? Genre { get; set; }
}
