namespace library_management.DTO
{
    public class MemberReportDto
    {
        public List<BorrowHistoryReportItem> BorrowHistory { get; set; }
        public List<CurrentBorrowedBookItem> CurrentBorrowedBooks { get; set; }
        public List<FineSummaryReportItem> FineSummary { get; set; }

        public MemberReportDto()
        {
            BorrowHistory = new List<BorrowHistoryReportItem>();
            CurrentBorrowedBooks = new List<CurrentBorrowedBookItem>();
            FineSummary = new List<FineSummaryReportItem>();
        }
    }

    // ✅ 1️⃣ Borrow History Report
    public class BorrowHistoryReportItem
    {
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? FineAmount { get; set; }
    }

    // ✅ 2️⃣ Current Borrowed Books
    public class CurrentBorrowedBookItem
    {
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public int DaysLeftForReturn { get; set; }
    }

    // ✅ 3️⃣ Fine Summary
    public class FineSummaryReportItem
    {
        public string BookTitle { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime DueDateForFinePayment { get; set; }
        public string PaymentStatus { get; set; }
    }
}
