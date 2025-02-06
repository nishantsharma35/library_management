using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class TblTab
{
    public int TabId { get; set; }

    public string TabName { get; set; } = null!;

    public string TabUrl { get; set; } = null!;

    public int? ParentId { get; set; }

    public string? IconPath { get; set; }

    public virtual ICollection<TblPermission> TblPermissions { get; set; } = new List<TblPermission>();
}
