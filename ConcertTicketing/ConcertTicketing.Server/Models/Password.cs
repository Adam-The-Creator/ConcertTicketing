using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

public partial class Password
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(72)]
    [Unicode(false)]
    public string HashedPassword { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string? Salt { get; set; }

    [InverseProperty("Password")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
