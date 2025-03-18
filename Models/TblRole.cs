using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class TblRole
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public bool? IsActive { get; set; }
}
