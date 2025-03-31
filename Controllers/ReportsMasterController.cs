using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library_management.Controllers
{
    public class ReportsMasterController : BaseController
    {
        private readonly dbConnect _context;
        //private readonly 

        public ReportsMasterController(dbConnect context,ISidebarRepository sidebar) : base(sidebar) 
        {
            _context = context;            
        }

        public IActionResult Index()
        {
            return View();
        }


        // 🔹 Super Admin - All Libraries Books Report
        public IActionResult AllLibrariesBooksReport()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            if (roleId != "1") return Unauthorized(); // Super Admin Only

            var report = _context.LibraryBooks
                .Include(lb => lb.Book)
                .Include(lb => lb.Library)
                .Select(lb => new
                {
                    LibraryName = lb.Library.Libraryname,
                    BookTitle = lb.Book.Title,
                    TotalStock = lb.Stock
                }).ToList();

            return View("SuperAdminReports", report);
        }

        // 🔹 Super Admin - All Libraries Fine Report
        public IActionResult AllLibrariesFineReport()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            if (roleId != "1") return Unauthorized(); // Super Admin Only

            var report = _context.Fines
                .Include(f => f.Borrow)
                .ThenInclude(b => b.Member)
                .Select(f => new
                {
                    MemberName = f.Borrow.Member.Name,
                    LibraryName = f.Borrow.Library.Libraryname,
                    FineAmount = f.FineAmount,
                    PaidAmount = f.PaidAmount,
                    Status = f.PaymentStatus
                }).ToList();

            return View("SuperAdminReports", report);
        }

        // 🔹 Library Admin - My Library Books Report
        public IActionResult MyLibraryBooksReport()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            var userLibraryId = User.Claims.FirstOrDefault(c => c.Type == "LibraryId")?.Value;
            if (roleId != "2" || string.IsNullOrEmpty(userLibraryId)) return Unauthorized(); // Library Admin Only

            int libraryId = int.Parse(userLibraryId);
            var report = _context.LibraryBooks
                .Where(lb => lb.LibraryId == libraryId)
                .Include(lb => lb.Book)
                .Select(lb => new
                {
                    BookTitle = lb.Book.Title,
                    TotalStock = lb.Stock
                }).ToList();

            return View(report);
        }

        // 🔹 Library Admin - My Library Fine Report
        public IActionResult MyLibraryFineReport()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            var userLibraryId = User.Claims.FirstOrDefault(c => c.Type == "LibraryId")?.Value;
            if (roleId != "2" || string.IsNullOrEmpty(userLibraryId)) return Unauthorized(); // Library Admin Only

            int libraryId = int.Parse(userLibraryId);
            var report = _context.Fines
                .Include(f => f.Borrow)
                .Where(f => f.Borrow.LibraryId == libraryId)
                .Select(f => new
                {
                    MemberName = f.Borrow.Member.Name,
                    FineAmount = f.FineAmount,
                    PaidAmount = f.PaidAmount,
                    Status = f.PaymentStatus
                }).ToList();

            return View(report);
        }

        // 🔹 Member - My Borrowed Books Report
        public IActionResult MyBorrowedBooks()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (roleId != "3" || string.IsNullOrEmpty(userId)) return Unauthorized(); // Member Only

            int memberId = int.Parse(userId);
            var report = _context.Borrows
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Book)
                .Select(b => new
                {
                    BookTitle = b.Book.Title,
                    BorrowDate = b.IssueDate,
                    ReturnDate = b.ReturnDate,
                    Status = b.IsReturned ? "Returned" : "Not Returned"
                }).ToList();

            return View(report);
        }

        // 🔹 Member - My Fine History Report
        public IActionResult MyFineHistory()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (roleId != "3" || string.IsNullOrEmpty(userId)) return Unauthorized(); // Member Only

            int memberId = int.Parse(userId);
            var report = _context.Fines
                .Where(f => f.Borrow.MemberId == memberId)
                .Select(f => new
                {
                    FineAmount = f.FineAmount,
                    PaidAmount = f.PaidAmount,
                    Status = f.PaymentStatus
                }).ToList();

            return View(report);
        }

    }

}
