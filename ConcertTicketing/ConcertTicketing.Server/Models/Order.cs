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
    public decimal DiscountedPrice { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [Column("DiscountID")]
    public Guid? DiscountId { get; set; }

    [Column("UserID")]
    public Guid? UserId { get; set; }

    [ForeignKey("DiscountId")]
    [InverseProperty("Orders")]
    public virtual Discount? Discount { get; set; }

    [InverseProperty("Orders")]
    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }
}
