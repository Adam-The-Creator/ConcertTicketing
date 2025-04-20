using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Models;

[Index("RoleName", Name = "UQ_Roles_Role", IsUnique = true)]
public partial class Role
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string RoleName { get; set; } = null!;

    [InverseProperty("Role")]
    public virtual ICollection<ConcertRole> ConcertRoles { get; set; } = new List<ConcertRole>();
}
