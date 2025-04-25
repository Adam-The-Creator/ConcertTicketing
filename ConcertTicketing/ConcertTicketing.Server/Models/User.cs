using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("Username", Name = "IX_Users_Username")]
[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("PasswordId", Name = "UQ_Users_PasswordID", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? SignedIn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("PasswordID")]
    public Guid? PasswordId { get; set; }

    [Column("UserRoleID")]
    public byte UserRoleId { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [ForeignKey("PasswordId")]
    [InverseProperty("User")]
    public virtual Password? Password { get; set; }

    [ForeignKey("UserRoleId")]
    [InverseProperty("Users")]
    public virtual UserRole UserRole { get; set; } = null!;
}
