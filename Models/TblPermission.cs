using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class TblPermission
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public int TabId { get; set; }

    public string PermissionType { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual TblTab Tab { get; set; } = null!;
}
