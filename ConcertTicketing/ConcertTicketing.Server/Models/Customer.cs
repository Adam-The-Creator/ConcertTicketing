using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Email", Name = "UQ_Customers_Email", IsUnique = true)]
public partial class Customer
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? SignedIn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("PasswordID")]
    public Guid? PasswordId { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [ForeignKey("PasswordId")]
    [InverseProperty("Customers")]
    public virtual Password? Password { get; set; }
}
