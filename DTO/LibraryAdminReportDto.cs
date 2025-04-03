namespace library_management.DTO
{
    public class LibraryAdminReportDto
    {
        
        public List<BookReportItem> LibraryBookReport { get; set; } = new();

   
        public List<BorrowedBookReportItem> BorrowedBooksReport { get; set; } = new();

       
        public List<MemberActivityReportItem> MemberActivityReport { get; set; } = new();

       
        public List<FineCollectionReportItem> FineCollectionReport { get; set; } = new();

        public TotalFineReportItem TotalFineReport { get; set; } = new();
    }

    // ✅ Library Book Report Item
    public class BookReportItem
    {
        public string BookTitle { get; set; }
        public int AvailableStock { get; set; }
        public int TotalBorrows { get; set; }
    }

    // ✅ Borrowed Books Report Item
    public class BorrowedBookReportItem
    {
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public decimal FineAmount { get; set; }
    }

    // ✅ Member Activity Report Item
    public class MemberActivityReportItem
    {
        public string MemberName { get; set; }
        public int TotalBooksBorrowed { get; set; }
        public int CurrentlyBorrowedBooks { get; set; }
        public int OverdueBooks { get; set; }
    }

    // ✅ Fine Collection Report Item
    public class FineCollectionReportItem
    {
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public decimal FineAmount { get; set; }
        public string FineStatus { get; set; } // "Paid" or "Pending"
        public DateTime? FinePaymentDate { get; set; }
    }

    // ✅ Total Fine Collected Report Item
    public class TotalFineReportItem
    {
        public decimal TotalFineCollected { get; set; }
        public decimal TotalPendingFines { get; set; }
    }
}
