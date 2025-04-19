using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Name", "Date", "VenueId", Name = "UQ_Concerts_Name_Date_VenueID", IsUnique = true)]
public partial class Concert
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(1024)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column("VenueID")]
    public int? VenueId { get; set; }

    [Column("MainArtistID")]
    public int? MainArtistId { get; set; }

    [Column("ConcertGroupID")]
    public int? ConcertGroupId { get; set; }

    [Column("StatusID")]
    public byte? StatusId { get; set; }

    [ForeignKey("ConcertGroupId")]
    [InverseProperty("Concerts")]
    public virtual ConcertGroup? ConcertGroup { get; set; }

    [InverseProperty("Concert")]
    public virtual ICollection<ConcertRole> ConcertRoles { get; set; } = new List<ConcertRole>();

    [ForeignKey("MainArtistId")]
    [InverseProperty("Concerts")]
    public virtual Artist? MainArtist { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Concerts")]
    public virtual ConcertStatus? Status { get; set; }

    [InverseProperty("Concert")]
    public virtual ICollection<TicketCategory> TicketCategories { get; set; } = new List<TicketCategory>();

    [InverseProperty("Concert")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [ForeignKey("VenueId")]
    [InverseProperty("Concerts")]
    public virtual Venue? Venue { get; set; }
}
