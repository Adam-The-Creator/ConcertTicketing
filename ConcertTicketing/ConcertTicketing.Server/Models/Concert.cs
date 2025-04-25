using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Date", Name = "IX_Concerts_Date")]
[Index("MainArtistId", Name = "IX_Concerts_MainArtistID")]
[Index("StatusId", Name = "IX_Concerts_StatusID")]
[Index("VenueId", Name = "IX_Concerts_VenueID")]
[Index("ConcertName", "Date", "VenueId", Name = "UQ_Concerts_Name_Date_VenueID", IsUnique = true)]
public partial class Concert
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [StringLength(256)]
    public string ConcertName { get; set; } = null!;

    [StringLength(1024)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column("VenueID")]
    public long VenueId { get; set; }

    [Column("MainArtistID")]
    public long? MainArtistId { get; set; }

    [Column("ConcertGroupID")]
    public int? ConcertGroupId { get; set; }

    [Column("StatusID")]
    public byte StatusId { get; set; }

    [InverseProperty("Concert")]
    public virtual ICollection<ArtistRolesAtConcert> ArtistRolesAtConcerts { get; set; } = new List<ArtistRolesAtConcert>();

    [ForeignKey("ConcertGroupId")]
    [InverseProperty("Concerts")]
    public virtual ConcertGroup? ConcertGroup { get; set; }

    [ForeignKey("MainArtistId")]
    [InverseProperty("Concerts")]
    public virtual Artist? MainArtist { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Concerts")]
    public virtual ConcertStatus Status { get; set; } = null!;

    [InverseProperty("Concert")]
    public virtual ICollection<TicketDetail> TicketDetails { get; set; } = new List<TicketDetail>();

    [InverseProperty("Concert")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [ForeignKey("VenueId")]
    [InverseProperty("Concerts")]
    public virtual Venue Venue { get; set; } = null!;
}
