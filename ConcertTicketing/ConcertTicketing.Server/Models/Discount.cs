using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

public partial class Discount
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public byte DiscountValue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDate { get; set; }

    [InverseProperty("Discount")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
