using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("RoleName", Name = "IX_UserRoles_RoleName")]
public partial class UserRole
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? RoleName { get; set; }

    [InverseProperty("UserRole")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
