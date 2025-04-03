namespace library_management.DTO
{
    public class SuperAdminReportsDto  // Parent DTO
    {
        public LibraryOverviewDto LibraryOverview { get; set; }
        public List<LibraryBookStockDto> LibraryBookStocks { get; set; }
        public List<LibraryBorrowedBooksDto> LibraryBorrowedBooks { get; set; }
        public List<LibraryAdminOverviewDto> LibraryAdmins { get; set; }
        public List<LibraryFineCollectionDto> LibraryFineCollections { get; set; }
        public List<MemberFineBreakdownDto> MemberFineBreakdowns { get; set; }

    }

    // 📌 Library Overview (Total libraries, books, members, fine)
    public class LibraryOverviewDto
    {
        public int TotalLibraries { get; set; }
        public int TotalBooks { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalFineCollected { get; set; }
    }

    // 📌 Library-Wise Book Stock
    public class LibraryBookStockDto
    {
        public string LibraryName { get; set; }
        public int TotalBooksAvailable { get; set; }
        public int TotalBorrowedBooks { get; set; }
    }

    // 📌 Library-Wise Borrowed Books
    public class LibraryBorrowedBooksDto
    {
        public string LibraryName { get; set; }
        public string BookTitle { get; set; }
        public string BorrowedBy { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public decimal? FineAmount { get; set; }
    }

    // 📌 Library Admins Overview
    public class LibraryAdminOverviewDto
    {
        public string LibraryName { get; set; }
        public string AdminName { get; set; }
        public int TotalBooksInLibrary { get; set; }
        public int TotalBorrowedBooks { get; set; }
        public int TotalActiveMembers { get; set; }
    }

    // 📌 Library-Wise Fine Collection
    public class LibraryFineCollectionDto
    {
        public string LibraryName { get; set; }
        public decimal TotalFineCollected { get; set; }
        public decimal TotalPendingFines { get; set; }
    }

    // 📌 Fine Breakdown (Member-Level Details)
    public class MemberFineBreakdownDto
    {
        public string LibraryName { get; set; }
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public decimal FineAmount { get; set; }
        public string FineStatus { get; set; }
        public DateTime? FinePaymentDate { get; set; }
    }

}
