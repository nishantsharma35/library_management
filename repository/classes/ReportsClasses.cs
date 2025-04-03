using library_management.DTO;
using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;

namespace library_management.repository.classes
{
    public class ReportsClasses : ReportsInterface
    {
        private readonly dbConnect _context;

        public ReportsClasses(dbConnect connect)
        {
            _context = connect;
        }

        public SuperAdminReportsDto GetSuperAdminReport(string libraryName, string bookTitle, int? minStock, int? maxStock, decimal? minFine, decimal? maxFine)
        {
            var report = new SuperAdminReportsDto();

            // 1️⃣ Library Overview (Total libraries, books, members, fine)
            report.LibraryOverview = new LibraryOverviewDto
            {
                TotalLibraries = _context.Libraries.Count(),
                TotalBooks = _context.Books.Count(),
                TotalMembers = _context.Members.Count(),
                TotalFineCollected = _context.Fines.Sum(f => (decimal?)f.FineAmount) ?? 0
            };

            // 2️⃣ Library-Wise Book Stock
            report.LibraryBookStocks = _context.LibraryBooks
                .GroupBy(lb => lb.Library.Libraryname)
                .Select(g => new LibraryBookStockDto
                {
                    LibraryName = g.Key,
                    TotalBooksAvailable = g.Sum(lb => lb.Stock),
                    TotalBorrowedBooks = _context.Borrows.Count(b => b.LibraryId == g.First().Library.LibraryId)
                }).ToList();

            // 3️⃣ Library-Wise Borrowed Books
            report.LibraryBorrowedBooks = _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.Member)
                .Include(b => b.Library)
                .Select(b => new LibraryBorrowedBooksDto
                {
                    LibraryName = b.Library.Libraryname,
                    BookTitle = b.Book.Title,
                    BorrowedBy = b.Member.Name,
                    BorrowDate = b.IssueDate,
                    ExpectedReturnDate = b.DueDate,
                    ActualReturnDate = b.ReturnDate,
                    FineAmount = b.FineAmount
                }).ToList();

            // 4️⃣ Library Admins Overview
            report.LibraryAdmins = _context.Libraries
    .Select(l => new LibraryAdminOverviewDto
    {
        LibraryName = l.Libraryname,   // ✅ Library ka naam
        AdminName = _context.Members
            .Where(m => m.LibraryId == l.LibraryId && m.RoleId == 2) // ✅ Sirf Admin Role wale members
            .Select(m => m.Name)
            .FirstOrDefault(), // ✅ Agar multiple admins ho to pehla admin ka naam fetch karega
        TotalBooksInLibrary = _context.LibraryBooks.Count(lb => lb.LibraryId == l.LibraryId),
        TotalBorrowedBooks = _context.Borrows.Count(b => b.LibraryId == l.LibraryId),
        TotalActiveMembers = _context.Members.Count(m => m.LibraryId == l.LibraryId)
    })
    .ToList();



            // 5️⃣ Library-Wise Fine Collection
            report.LibraryFineCollections = _context.Libraries
                .Select(l => new LibraryFineCollectionDto
                {
                    LibraryName = l.Libraryname,
                    TotalFineCollected = _context.Fines.Where(f => f.Borrow.LibraryId == l.LibraryId).Sum(f => (decimal?)f.FineAmount) ?? 0,
                    TotalPendingFines = _context.Fines.Where(f => f.Borrow.LibraryId == l.LibraryId && f.PaymentStatus == "Pending").Sum(f => (decimal?)f.FineAmount) ?? 0
                }).ToList();

            // 6️⃣ Fine Breakdown (Member-Level Details)
            report.MemberFineBreakdowns = _context.Fines
                .Include(f => f.Borrow)
                .Include(f => f.Borrow.Book)
                .Include(f => f.Borrow.Member)
                .Select(f => new MemberFineBreakdownDto
                {
                    LibraryName = f.Borrow.Library.Libraryname,
                    MemberName = f.Borrow.Member.Name,
                    BookTitle = f.Borrow.Book.Title,
                    FineAmount = f.FineAmount,
                    FineStatus = f.PaymentStatus,
                    FinePaymentDate = f.PaymentDate
                }).ToList() ?? new List<MemberFineBreakdownDto>();
            return report;
        }


        public LibraryAdminReportDto GetLibraryAdminReport(int libraryId)
        {
            var report = new LibraryAdminReportDto();

            // ✅ Library Book Report
            report.LibraryBookReport = _context.LibraryBooks
                .Where(lb => lb.LibraryId == libraryId)
                .Select(lb => new BookReportItem
                {
                    BookTitle = lb.Book.Title,
                    AvailableStock = lb.Stock,
                    TotalBorrows = _context.Borrows.Count(b => b.BookId == lb.BookId && b.LibraryId == libraryId)
                }).ToList();

            // ✅ Borrowed Books Report
            report.BorrowedBooksReport = _context.Borrows
                .Where(b => b.LibraryId == libraryId)
                .Include(b => b.Book)
                 .Include(b => b.Member)
                  .Select(b => new BorrowedBookReportItem
                  {
                      MemberName = b.Member.Name,
                      BookTitle = b.Book.Title,
                      BorrowDate = b.IssueDate,
                      ExpectedReturnDate = b.DueDate,
                      ActualReturnDate = b.ReturnDate,
                      FineAmount = _context.Fines
                       .Where(f => f.BorrowId == b.BorrowId) // ✅ Borrow se Fine ka relation
                       .Select(f => f.FineAmount)
                       .FirstOrDefault() // ✅ Agar fine nahi mila to default 0
                  }).ToList();


            // ✅ Member Activity Report
            report.MemberActivityReport = _context.Members
                .Where(m => m.LibraryId == libraryId)
                .Select(m => new MemberActivityReportItem
                {
                    MemberName = m.Name,
                    TotalBooksBorrowed = _context.Borrows.Count(b => b.MemberId == m.Id),
                    CurrentlyBorrowedBooks = _context.Borrows.Count(b => b.MemberId == m.Id && b.ReturnDate == null),
                    OverdueBooks = _context.Borrows.Count(b => b.MemberId == m.Id && b.DueDate < DateTime.Now && b.ReturnDate == null)
                }).ToList();

            // ✅ Fine Collection Report (Library Admin Specific)
            report.FineCollectionReport = _context.Fines
                .Where(f => f.Borrow.LibraryId == libraryId)
                .Select(f => new FineCollectionReportItem
                {
                    MemberName = f.Borrow.Member.Name,
                    BookTitle = f.Borrow.Book.Title,
                    FineAmount = f.FineAmount,
                    FineStatus = f.PaymentStatus,
                    FinePaymentDate = f.PaymentDate
                }).ToList();

            // ✅ Total Fine Collected Report
            report.TotalFineReport = new TotalFineReportItem
            {
                TotalFineCollected = _context.Fines.Where(f => f.Borrow.LibraryId == libraryId && f.PaymentStatus == "Paid").Sum(f => (decimal?)f.FineAmount) ?? 0,
                TotalPendingFines = _context.Fines.Where(f => f.Borrow.LibraryId == libraryId && f.PaymentStatus == "Pending").Sum(f => (decimal?)f.FineAmount) ?? 0
            };

            return report;
        }

        public MemberReportDto GetMemberReport(int memberId)
        {
            var report = new MemberReportDto();

            // ✅ 1️⃣ Borrow History (Returned Books)
            report.BorrowHistory = _context.Borrows
                .Where(b => b.MemberId == memberId && b.ReturnDate != null)
                .Include(b => b.Book)
                .Select(b => new BorrowHistoryReportItem
                {
                    BookTitle = b.Book.Title,
                    BorrowDate = b.IssueDate,
                    ReturnDate = b.ReturnDate,
                    FineAmount = _context.Fines
                        .Where(f => f.BorrowId == b.BorrowId)
                        .Sum(f => (decimal?)f.FineAmount) ?? 0
                }).ToList();

            // ✅ 2️⃣ Current Borrowed Books (Active Borrows)
            report.CurrentBorrowedBooks = _context.Borrows
                .Where(b => b.MemberId == memberId && b.ReturnDate == null)
                .Include(b => b.Book)
                .Select(b => new CurrentBorrowedBookItem
                {
                    BookTitle = b.Book.Title,
                    BorrowDate = b.IssueDate,
                    ExpectedReturnDate = b.DueDate,
                    DaysLeftForReturn = (b.DueDate - DateTime.Now).Days
                }).ToList();

            // ✅ 3️⃣ Fine Summary (Pending Fines)
            report.FineSummary = _context.Fines
                .Where(f => f.Borrow.MemberId == memberId)
                .Include(f => f.Borrow.Book)
                .Select(f => new FineSummaryReportItem
                {
                    BookTitle = f.Borrow.Book.Title,
                    FineAmount = f.FineAmount,
                    DueDateForFinePayment = f.Borrow.DueDate,
                    PaymentStatus = f.PaymentStatus
                }).ToList();

            return report;
        }
    }
}
