using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models;

public partial class Library
{
    public int LibraryId { get; set; }

    public string? Libraryname { get; set; }

    public int AdminId { get; set; }

    public string? LibraryImagePath { get; set; }
    [NotMapped]
    public IFormFile? libraryfile { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public long? Pincode { get; set; }

    public string? Address { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? ClosingTime { get; set; }

    public decimal LibraryFineAmount { get; set; }

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual ICollection<LibraryBook> LibraryBooks { get; set; } = new List<LibraryBook>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    
    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

}