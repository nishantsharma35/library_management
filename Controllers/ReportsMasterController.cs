using ClosedXML.Excel;
using library_management.DTO;
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
        private readonly ReportsInterface _reportService;
        private readonly IActivityRepository _activityRepository;

        public ReportsMasterController(dbConnect context,ReportsInterface reportsInterface ,ISidebarRepository sidebar, IActivityRepository activityRepository) : base(sidebar)
        {
            _context = context;
            _reportService = reportsInterface;
            _activityRepository = activityRepository;
        }

        [HttpGet]
        public IActionResult SuperAdminReport(string libraryName, string bookTitle, int? minStock, int? maxStock, decimal? minFine, decimal? maxFine)
        {
            var roleId = HttpContext.Session.GetInt32("UserRoleId");
            if (roleId != 1) return Unauthorized(); // Only Super Admin

            var report = _reportService.GetSuperAdminReport(libraryName, bookTitle, minStock, maxStock, minFine, maxFine);

            return View(report); // ✅ Make sure this is List<SuperAdminReportDto>
        }




        [HttpGet]
        public IActionResult LibraryAdminReport()
        {
            var roleId = HttpContext.Session.GetInt32("UserRoleId");
            var libraryId = HttpContext.Session.GetInt32("LibraryId");

            if (roleId != 2 || libraryId == null) return Unauthorized(); // Only Library Admin

            LibraryAdminReportDto report = _reportService.GetLibraryAdminReport(libraryId.Value);
            return View(report);
        }


        [HttpGet]
        public IActionResult MemberReport()
        {
            var roleId = HttpContext.Session.GetInt32("UserRoleId");
            var memberId = HttpContext.Session.GetInt32("MemberId");

            if (roleId != 3 || memberId == null) return Unauthorized(); // Only Member

           MemberReportDto report = _reportService.GetMemberReport(memberId.Value);
            return View(report);
        }

        public IActionResult ExportToExcel()
        {
            var reportData =  _reportService. GetSuperAdminReport("library1", "book1", null, null, null, null);
            var fineBreakdowns = reportData.MemberFineBreakdowns; // ✅ Ab error nahi aayega

            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SuperAdminReports");
                int row = 1;

                // Headers
                worksheet.Cell(row, 1).Value = "Library Name";
                worksheet.Cell(row, 2).Value = "Member Name";
                worksheet.Cell(row, 3).Value = "Book Title";
                worksheet.Cell(row, 4).Value = "Fine Amount";
                worksheet.Cell(row, 5).Value = "Fine Status";
                worksheet.Cell(row, 6).Value = "Fine Payment Date";

                row++;

                // Data Rows
                foreach (var item in reportData.MemberFineBreakdowns)
                {
                    worksheet.Cell(row, 1).Value = item.LibraryName;
                    worksheet.Cell(row, 2).Value = item.MemberName;
                    worksheet.Cell(row, 3).Value = item.BookTitle;
                    worksheet.Cell(row, 4).Value = item.FineAmount;
                    worksheet.Cell(row, 5).Value = item.FineStatus;
                    worksheet.Cell(row, 6).Value = item.FinePaymentDate?.ToString("dd-MM-yyyy") ?? "N/A";
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string type = "superadmin exported file";
                    string desc = $"{userName} exported the data file";
                    _activityRepository.AddNewActivity(id, type, desc);

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SuperAdminReports.xlsx");
                }
            }
        }

        


        public IActionResult ExportLibraryAdminReport()
        {

            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            // ✅ Library Admin ke liye sirf uski library ka data fetch karna hai
            int libraryId = (int)HttpContext.Session.GetInt32("LibraryId");
            var reportData = _reportService.GetLibraryAdminReport(libraryId);
            if (reportData == null || !reportData.BorrowedBooksReport.Any())
            {
                throw new Exception("No data available for export.");
            }


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("LibraryAdminReports");
                int row = 1;

                // 🔹 Headers
                worksheet.Cell(row, 1).Value = "Member Name";
                worksheet.Cell(row, 2).Value = "Book Title";
                worksheet.Cell(row, 3).Value = "Borrow Date";
                worksheet.Cell(row, 4).Value = "Return Date";
                worksheet.Cell(row, 5).Value = "Fine Amount";
                row++;

                // 🔹 Data Rows
                foreach (var item in reportData.BorrowedBooksReport)
                {
                    worksheet.Cell(row, 1).Value = item.MemberName;
                    worksheet.Cell(row, 2).Value = item.BookTitle;
                    worksheet.Cell(row, 3).Value = item.BorrowDate.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 4).Value = item.ActualReturnDate?.ToString("dd-MM-yyyy") ?? "N/A";
                    worksheet.Cell(row, 5).Value = item.FineAmount;
                    row++;
                }

                // 🔹 Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string type = "admin exported file";
                    string desc = $"{userName} exported the data file library {libName}";
                    _activityRepository.AddNewActivity(id, type, desc);

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LibraryAdminReports.xlsx");
                }
            }
        }
        [HttpGet]
        public IActionResult ExportMemberReport()
        {

            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null) return Unauthorized();

            var report = _reportService.GetMemberReport(memberId.Value);
          
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Member Report");
                int row = 1;

                // 📌 Borrow History
                worksheet.Cell(row, 1).Value = "Book Title";
                worksheet.Cell(row, 2).Value = "Borrow Date";
                worksheet.Cell(row, 3).Value = "Return Date";
                worksheet.Cell(row, 4).Value = "Fine Amount";
                row++;


                foreach (var borrow in report.BorrowHistory)
                {
                    worksheet.Cell(row, 1).Value = borrow.BookTitle;
                    worksheet.Cell(row, 2).Value = borrow.BorrowDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 3).Value = borrow.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned";
                    worksheet.Cell(row, 4).Value = borrow.FineAmount ?? 0;
                    row++;
                }

                // 📌 Current Borrowed Books
                worksheet.Cells("A" + (row + 2)).Value = "Book Title";
                worksheet.Cells("B" + (row + 2)).Value = "Borrow Date";
                worksheet.Cells("C" + (row + 2)).Value = "Expected Return Date";
                worksheet.Cells("D" + (row + 2)).Value = "Days Left";

                row += 3;
                foreach (var borrow in report.CurrentBorrowedBooks)
                {
                    worksheet.Cell(row, 1).Value = borrow.BookTitle;
                    worksheet.Cell(row, 2).Value = borrow.BorrowDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 3).Value = borrow.ExpectedReturnDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = (borrow.ExpectedReturnDate - DateTime.Now).Days;
                    row++;
                }

                // 📌 Fine Summary
                worksheet.Cells("A" + (row + 2)).Value = "Book Title";
                worksheet.Cells("B" + (row + 2)).Value = "Fine Amount";
                worksheet.Cells("D" + (row + 2)).Value = "Payment Status";

                row += 3;
                foreach (var fine in report.FineSummary)
                {
                    worksheet.Cell(row, 1).Value = fine.BookTitle;
                    worksheet.Cell(row, 2).Value = fine.FineAmount;
                    worksheet.Cell(row, 4).Value = fine.PaymentStatus;
                    row++;
                }

                // 📤 Send File
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string type = "member exported file";
                    string desc = $"{userName} exported the data file";
                    _activityRepository.AddNewActivity(id, type, desc);



                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Member_Report.xlsx");
                }
            }
        }


    }
}
