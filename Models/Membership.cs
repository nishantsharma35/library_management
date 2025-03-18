using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class Membership
{
    public int MembershipId { get; set; }

    public int MemberId { get; set; }

    public int LibraryId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Library Library { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
