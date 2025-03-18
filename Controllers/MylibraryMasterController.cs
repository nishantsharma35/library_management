using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace library_management.Controllers
{
    public class MylibraryMasterController : BaseController
    {
        private readonly dbConnect _context;
        private readonly BorrowInterface _borrowInterface;
        private readonly FineInterface _fineInterface;
        private readonly EmailSenderInterface _emailService;
        public MylibraryMasterController(ISidebarRepository sidebar, BorrowInterface borrowInterface, dbConnect context, FineInterface fineInterface, EmailSenderInterface emailService) : base(sidebar)
        {
            _borrowInterface = borrowInterface;
            _context = context;
            _fineInterface = fineInterface;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AvailableBooks()
        {
            int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;

            if (memberId == 0)
            {
                return Unauthorized(); // Agar Member login nahi hai
            }

            // **1️⃣ Member ki registered libraries ko fetch karo**
            var memberLibraryIds = _context.Memberships
                .Where(m => m.MemberId == memberId)
                .Select(m => m.LibraryId)
                .Distinct() // Unique libraries
                .ToList();

            Console.WriteLine("Member Registered Library IDs: " + string.Join(", ", memberLibraryIds));

            // **2️⃣ Sirf wahi libraries fetch karo jo member ki hain**
            var availableLibraries = _context.Libraries
                .Where(l => memberLibraryIds.Contains(l.LibraryId))
                .Select(l => new SelectListItem
                {
                    Value = l.LibraryId.ToString(),
                    Text = l.Libraryname
                })
                .ToList();

            Console.WriteLine("Total Libraries in Dropdown: " + availableLibraries.Count);

            ViewBag.Libraries = availableLibraries;

            // **3️⃣ Member ki selected library ka data fetch karo**
            var availableBooks = _context.LibraryBooks
                .Where(lb => memberLibraryIds.Contains(lb.LibraryId) && lb.Stock > 0)
                .Include(lb => lb.Book) // Book Details fetch karne ke liye
                .Select(lb => lb.Book)
                .ToList();

            return View(availableBooks);
        }


        [HttpGet]
        public IActionResult BorrowedBooks()
        {
            int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;

            if (memberId == 0)
            {
                return Unauthorized();
            }

            var borrowedBooks = _context.Borrows
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Book)
                .Include(b => b.Fine)
                .OrderByDescending(b => b.IssueDate)
                .ToList();

            return View(borrowedBooks);


        }

        [HttpPost]
        public IActionResult BorrowBook(int bookId)
        {
            int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;
            int libraryId = GetLoggedInLibraryId();

            if (memberId == 0 || libraryId == 0)
            {
                return Unauthorized();
            }

            // Check if book is available in the library
            var libraryBook = _context.LibraryBooks.FirstOrDefault(lb => lb.BookId == bookId && lb.LibraryId == libraryId);

            if (libraryBook == null)
            {
                TempData["Error"] = "Invalid book selection.";
                return RedirectToAction("AvailableBooks");
            }

            // **Create borrow request (without reducing stock)**
            var borrowRequest = new Borrow
            {
                MemberId = memberId,
                LibraryId = libraryId,
                BookId = bookId,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7), // Default 7 days
                Status = "Pending Request" // 🔹 Request Pending
            };

            _context.Borrows.Add(borrowRequest);
            _context.SaveChanges();

            Console.WriteLine($"✅ Borrow Request Inserted: Member {memberId} | Library {libraryId} | Book {bookId}");

            TempData["Success"] = "Borrow request sent successfully! Waiting for admin approval.";
            return RedirectToAction("AvailableBooks");
        }


        private int GetLoggedInLibraryId()
        {
            int? libraryId = HttpContext.Session.GetInt32("LibraryId");

            if (libraryId == null || libraryId == 0)
            {
                int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0; // 🔹 Logged-in Member ID le lo

                // 🔹 Membership table se LibraryId fetch karo
                libraryId = _context.Memberships
                    .Where(m => m.MemberId == memberId)
                    .Select(m => m.LibraryId)
                    .FirstOrDefault(); // Agar multiple libraries hai, toh sabse pehli wala lega

                if (libraryId != null && libraryId > 0)
                {
                    HttpContext.Session.SetInt32("LibraryId", libraryId.Value); // ✅ Session me LibraryId store karo
                    Console.WriteLine("✅ LibraryId set from Membership Table: " + libraryId);
                }
                else
                {
                    Console.WriteLine("❌ No LibraryId found for MemberId: " + memberId);
                }
            }

            Console.WriteLine("📌 Final LibraryId from Session: " + (libraryId ?? 0));
            return libraryId ?? 0;
        }



        //[HttpPost]
        //public IActionResult RequestReturn(int bookId)
        //{
        //    int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;
        //    int libraryId = GetLoggedInLibraryId();

        //    if (memberId == 0 || libraryId == 0)
        //    {
        //        return Unauthorized();
        //    }

        //    var borrowRecord = _context.Borrows
        //        .FirstOrDefault(b => b.MemberId == memberId && b.BookId == bookId && b.LibraryId == libraryId && b.Status == "Borrowed");

        //    if (borrowRecord == null)
        //    {
        //        TempData["Error"] = "Invalid return request.";
        //        return RedirectToAction("BorrowedBooks");
        //    }

        //    // Update Status to 'Pending Return'
        //    borrowRecord.Status = "Pending Return";
        //    _context.SaveChanges();

        //    TempData["Success"] = "Return request sent to Admin.";
        //    return RedirectToAction("BorrowedBooks");
        //}



        [HttpPost]
        public IActionResult RequestReturn(int borrowId)
        {
            int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;
            int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;


            if (memberId == 0 || libraryId == 0)
            {
                return Unauthorized();
            }

            var borrowRecord = _context.Borrows
                .FirstOrDefault(b => b.BorrowId == borrowId && b.MemberId == memberId && b.LibraryId == libraryId && b.Status == "Borrowed");

            if (borrowRecord == null)
            {
                TempData["Error"] = "Invalid return request.";
                return RedirectToAction("BorrowedBooks");
            }

            // Update Status to 'Pending Return'
            borrowRecord.Status = "Pending Return";
            _context.SaveChanges();

            TempData["Success"] = "Return request sent to Admin.";
            return RedirectToAction("BorrowedBooks");
        }
        [HttpPost]
        public async Task<IActionResult> PayFine(int fineId, decimal payAmount)
        {
            int memberId = HttpContext.Session.GetInt32("MemberId") ?? 0; // Get logged-in member ID

            if (memberId == 0)
            {
                TempData["Error"] = "Session expired. Please log in again!";
                return RedirectToAction("Login", "Account");
            }

            var fine = await _fineInterface.GetFineByIdAsync(fineId);
            if (fine == null || fine.Borrow == null || fine.Borrow.MemberId != memberId)
            {
                TempData["Error"] = "Invalid fine details!";
                return RedirectToAction("BorrowedBooks");
            }

            // 🔹 Ensure LibraryId is fetched properly
            int libraryId = fine.Borrow?.LibraryId ?? 0; // Null ho toh default 0 set hoga

            if (libraryId == 0)
            {
                TempData["Error"] = "Library information missing!";
                return RedirectToAction("BorrowedBooks");
            }

            // Check if payment is valid
            decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
            if (payAmount > remainingAmount)
            {
                TempData["Error"] = "Payment exceeds remaining fine!";
                return RedirectToAction("BorrowedBooks");
            }

            // Ensure LibraryId is set before updating
            fine.LibraryId = libraryId;

            // Update fine details
            fine.PaidAmount += payAmount;
            fine.PaymentDate = DateTime.Now;
            fine.PaymentStatus = fine.PaidAmount >= fine.FineAmount ? "Paid" : "Partially Paid";

            await _fineInterface.UpdateFineAsync(fine);

            // Check if LibraryId is stored properly
            //debug.WriteLine($"[DEBUG] LibraryId: {fine.LibraryId} stored in DB");

            // Send email confirmation to member
            string subject = "Fine Payment Confirmation";
            string memberName = fine.Borrow?.Member?.Name ?? "Guest"; // Default to "Guest" if Member is null

            string body = fine.Borrow?.Member != null
                ? $"Dear {memberName},\\n\\n" +
                  $"You have made a payment of ₹{payAmount} towards your fine.\\n\\n" +
                  $"Remaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\\n\\n"
                : "Dear Member,\\n\\nYour fine has been updated.";

            // Append payment status message
            body += fine.PaymentStatus == "Paid"
                ? "Your fine has been fully paid. Thank you for using our service!"
                : "Please pay the remaining amount to fully clear your fine.";

            string recipientEmail = fine.Borrow?.Member?.Email ?? "defaultEmail@example.com"; // Default email

            await _emailService.SendEmailAsync(recipientEmail, subject, body);

            TempData["Success"] = "Fine payment successful!";
            return RedirectToAction("BorrowedBooks");
        }

        [HttpGet]
        public IActionResult GetBooksByLibrary(int libraryId)
        {
            if (libraryId == 0)
            {
                return BadRequest("Invalid Library ID");
            }

            var books = _context.LibraryBooks
                .Where(lb => lb.LibraryId == libraryId && lb.Stock > 0)
                .Include(lb => lb.Book)
                .Select(lb => new
                {
                    lb.Book.BookId,
                    lb.Book.Title,
                    lb.Book.Author
                })
                .ToList();

            return Json(books);
        }



    }

}

