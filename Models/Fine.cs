using System;
using System.Collections.Generic;

namespace library_management.Models;

public partial class Fine
{
    public int FineId { get; set; }

    public int BorrowId { get; set; }

    public decimal FineAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public string? PaymentStatus { get; set; } = "Pending";

    public DateTime? PaymentDate { get; set; }

    public int LibraryId { get; set; }
    
    public virtual Borrow Borrow { get; set; } = null!;

    public virtual Library? Library { get; set; }
    public virtual ICollection<TblTransaction> TblTransactions { get; set; } = new List<TblTransaction>();



}
