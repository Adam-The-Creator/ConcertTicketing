using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Status", Name = "UQ_DiscountStatuses_Status", IsUnique = true)]
public partial class DiscountStatus
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [InverseProperty("Status")]
    public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}
