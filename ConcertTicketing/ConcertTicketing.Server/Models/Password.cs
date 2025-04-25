using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("HashedPassword", Name = "IX_Passwords_HashedPassword")]
public partial class Password
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(72)]
    [Unicode(false)]
    public string HashedPassword { get; set; } = null!;

    [InverseProperty("Password")]
    public virtual User? User { get; set; }
}
