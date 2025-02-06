using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models;

public partial class Library
{
    public int LibraryId { get; set; }

    public string? Libraryname { get; set; }

    public int AdminId { get; set; }
    [NotMapped]
    public IFormFile? libraryfile { get; set; }

    public string? LibraryImagePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public long? Pincode { get; set; }

    public string? Address { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? ClosingTime { get; set; }
}
