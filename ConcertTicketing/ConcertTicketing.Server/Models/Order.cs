using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

public partial class Order
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string? DeliveryAddress { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string? DeliveryEmail { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PreferredDeliveryTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Paid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Sent { get; set; }

    [Column(TypeName = "money")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? Discount { get; set; }

    [Column(TypeName = "money")]
    public decimal DiscountedPrice { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [Column("CustomerID")]
    public Guid? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Orders")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("Orders")]
    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
