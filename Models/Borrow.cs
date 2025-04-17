using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models;

public partial class Borrow
{
    public int BorrowId { get; set; }

    public int MemberId { get; set; }

    public int BookId { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string? Status { get; set; } 

    public bool IsReturned { get; set; }

    public int LibraryId { get; set; }
    [NotMapped]  // Ye database me store nahi hoga, sirf runtime pe use hoga
    public decimal FineAmount { get; set; }
    public string? otp { get; set; }

    public DateTime? otpexpires { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Fine? Fine { get; set; }

    public virtual Library? Library { get; set; }

    public virtual Member Member { get; set; } = null!;
}
