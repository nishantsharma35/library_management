using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library_management.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly dbConnect _context;
        public DashboardController(dbConnect context, ISidebarRepository sidebar) : base(sidebar)
        {
            _context = context;
        }

        [Route("dashboard")]

        public IActionResult Index()
        {
            var roleId = HttpContext.Session.GetInt32("UserRoleId");
            var userId = HttpContext.Session.GetInt32("UserId");
            var model = new DashboardViewModel();
            model.RoleId = roleId;
            if (roleId == 1)
            {
                model.TotalLibraries = _context.Libraries.Count();
                model.TotalAdmins = _context.Libraries.Count(m => m.AdminId != 0);
                model.TotalMembers = _context.Members.Count();
                model.PendingAdminApprovals = _context.Members.Count(m => m.RoleId == 2 && m.VerificationStatus != "Accepted");
                model.PendingMemberApprovals = _context.Members.Count(m => m.VerificationStatus != "Accepted");
            }
            else if (roleId == 2) // Admin
            {
                var library = _context.Libraries.FirstOrDefault(l => l.AdminId == userId);

                if (library != null)
                {
                    int libId = library.LibraryId;

                    model.TotalMembers = _context.Members
                   .Count(m => m.RoleId == 3 &&
                  m.VerificationStatus == "Accepted" &&
                  _context.Memberships.Any(ms => ms.MemberId == m.Id && ms.LibraryId == libId));


                    model.TotalBooks = _context.LibraryBooks
                        .Where(lb => lb.LibraryId == libId)
                        .Sum(lb => lb.Stock);

                    model.TotalDistinctBooks = _context.LibraryBooks
                    .Where(lb => lb.LibraryId == libId)
                    .Select(lb => lb.BookId)
                    .Distinct()
                    .Count();

                    model.TotalBorrows = _context.Borrows
                        .Count(b => b.LibraryId == libId);

                    model.OverdueBooks = _context.Borrows
                        .Count(b => b.LibraryId == libId && b.ReturnDate == null && b.DueDate < DateTime.Now);

                }
            }
            return View(model); // Make sure this line is present
        }

        [HttpGet]
        public IActionResult GetMonthlyUserStats(int year, int libraryId)
        {
            // Fetch all members joined in selected year
            var allMonthlyUsers = _context.Members
                .Where(u => u.Joiningdate.HasValue &&
                            u.Joiningdate.Value.Year == year)
                .ToList();

            var stats = new List<DashboardViewModel>();

            for (int month = 1; month <= 12; month++)
            {
                var monthlyUsers = allMonthlyUsers
                    .Where(u => u.Joiningdate.Value.Month == month)
                    .ToList();

                if (monthlyUsers.Any()) // ✅ Only add if data exists for the month
                {
                    int adminCount = monthlyUsers.Count(u => u.RoleId == 2);
                    int memberCount = monthlyUsers.Count(u => u.RoleId == 3);

                    stats.Add(new DashboardViewModel
                    {
                        Month = new DateTime(year, month, 1).ToString("MMMM"),
                        AdminCount = adminCount,
                        MemberCount = memberCount
                    });
                }
            }

            var labels = stats.Select(s => s.Month).ToList();
            var adminCounts = stats.Select(s => s.AdminCount).ToList();
            var memberCounts = stats.Select(s => s.MemberCount).ToList();

            return Json(new { labels, adminCounts, memberCounts });
        }



        //[HttpGet]
        //public IActionResult GetMonthlyUserStats(int year,int libraryId)
        //{

        //    var currentMonth = 12; // Full year ke liye


        //    // Create a class for monthly stats
        //    var stats = new List<DashboardViewModel>();

        //    for (int month = 1; month <= currentMonth; month++)
        //    {
        //        var monthlyUsers = _context.Members
        //            .Where(u => u.Joiningdate.HasValue &&
        //                        u.Joiningdate.Value.Year == year &&
        //                        u.Joiningdate.Value.Month == month)
        //            .ToList();

        //        int adminCount = monthlyUsers.Count(u => u.RoleId == 2);
        //        int memberCount = monthlyUsers.Count(u => u.RoleId == 3);

        //        stats.Add(new DashboardViewModel
        //        {
        //            Month = new DateTime(year, month, 1).ToString("MMMM"),
        //            AdminCount = adminCount,
        //            MemberCount = memberCount
        //        });
        //    }

        //    var labels = stats.Select(s => s.Month).ToList();
        //    var adminCounts = stats.Select(s => s.AdminCount).ToList();
        //    var memberCounts = stats.Select(s => s.MemberCount).ToList();

        //    return Json(new { labels, adminCounts, memberCounts });
        //}


        [HttpGet]
        public IActionResult GetMonthlyBorrowReturnStats(int year, int libraryId)
        {
            libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;
            ViewBag.LibraryId = libraryId;
            var stats = new List<DashboardViewModel>();

            for (int month = 1; month <= 12; month++)
            {
                int borrowCount = _context.Borrows
                    .Count(b => b.IssueDate.Year == year &&
                                b.IssueDate.Month == month &&
                                b.LibraryId == libraryId);

                int returnCount = _context.Borrows
                    .Count(b => b.ReturnDate.HasValue &&
                                b.ReturnDate.Value.Year == year &&
                                b.ReturnDate.Value.Month == month &&
                                b.LibraryId == libraryId);

                if (borrowCount > 0 || returnCount > 0)
                {
                    stats.Add(new DashboardViewModel
                    {
                        Month = new DateTime(year, month, 1).ToString("MMMM"),
                        BorrowCount = borrowCount,
                        ReturnCount = returnCount
                    });
                }
            }

            var labels = stats.Select(s => s.Month).ToList();
            var borrowCounts = stats.Select(s => s.BorrowCount).ToList();
            var returnCounts = stats.Select(s => s.ReturnCount).ToList();

            return Json(new { labels, borrowCounts, returnCounts });
        }

        [HttpGet]
        public IActionResult GetFinePaymentStatus()
        {
            int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;

            var fines = _context.Fines
                .Include(f => f.Borrow)
                .Where(f => f.Borrow.LibraryId == libraryId)
                .ToList();

            // Avoid null value issues using null checks
            int paid = fines.Count(f =>
                f.PaidAmount != null && f.FineAmount != null &&
                f.PaidAmount >= f.FineAmount);

            int unpaid = fines.Count(f =>
                f.PaidAmount != null && f.FineAmount != null &&
                f.PaidAmount < f.FineAmount);

            var labels = new[] { "Paid", "Unpaid" };
            var values = new[] { paid, unpaid };

            return Json(new { labels, values });
        }



    }
}


