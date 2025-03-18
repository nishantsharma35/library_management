using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class LibraryBook
{
    public int LibraryBookId { get; set; }

    public int BookId { get; set; }

    public int LibraryId { get; set; }

    public int Stock { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Library Library { get; set; } = null!;
}
