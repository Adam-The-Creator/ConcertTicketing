using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("GenreName", Name = "UQ_Genres_Name", IsUnique = true)]
public partial class Genre
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string GenreName { get; set; } = null!;

    [InverseProperty("Genre")]
    public virtual ICollection<GenresOfArtist> GenresOfArtists { get; set; } = new List<GenresOfArtist>();
}
