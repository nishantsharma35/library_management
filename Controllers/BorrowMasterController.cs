using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace library_management.Controllers
{
    public class BorrowMasterController : BaseController
    {
        private readonly dbConnect _connect;
        private readonly libraryInterface _libraryInterface;
        private readonly BookServiceInterface _bookServiceInterface;
        private readonly BorrowInterface _borrowInterface;
        private readonly FineInterface _fineInterface;
        private readonly MembershipInterface _membershipInterface;
        private readonly EmailSenderInterface _emailSenderInterface;
        private readonly PermisionHelperInterface _permission;
        public BorrowMasterController(dbConnect connect,ISidebarRepository sidebar, libraryInterface libraryInterface, BookServiceInterface bookServiceInterface,BorrowInterface borrowInterface,FineInterface fineInterface,MembershipInterface membershipInterface,EmailSenderInterface emailSenderInterface,PermisionHelperInterface permission ) : base(sidebar)
        {
            _connect = connect;
            _libraryInterface = libraryInterface;
            _bookServiceInterface = bookServiceInterface;
            _borrowInterface = borrowInterface;
            _fineInterface = fineInterface;
            _membershipInterface = membershipInterface;
            _emailSenderInterface = emailSenderInterface;
            _permission = permission;
        }

        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        public async Task<IActionResult> BorrowList()
        {
            string permissionType = GetUserPermission("Borrower list");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                try
                {
                    int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;
                    var borrowedBooks = await _borrowInterface.GetAllBorrowsAsync(libraryId);

                    // Filter out pending requests and show only borrowed/returned books
                    borrowedBooks = borrowedBooks.Where(b =>
                        b.Book != null &&
                        b.Member != null &&
                        (b.Status == "Borrowed" || b.Status == "Returned")
                    ).ToList();

                    foreach (var borrow in borrowedBooks)
                    {
                        var fine = await _fineInterface.GetFineByBorrowIdAsync(borrow.BorrowId);
                        borrow.Fine = fine;
                    }
                    return View(borrowedBooks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in BorrowList: {ex.Message}");
                    return View(new List<Borrow>());
                }

            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        

        [HttpGet]
        public async Task<IActionResult> AddBorrow()
            {
            string permissionType = GetUserPermission("Add Borrower");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0; // Librarian ki LibraryId
                int userRoleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;

                List<Member> members;
                List<Book> books;

                if (userRoleId == 1)
                {
                    members = await _libraryInterface.GetAllMembersAsync();
                    books = await _bookServiceInterface.GetAvailableBooksAsync();
                }
                else
                {
                    members = await _libraryInterface.GetLibraryMembersAsync(libraryId);
                    books = await _bookServiceInterface.GetAvailableBooksByLibraryAsync(libraryId);
                }

                Console.WriteLine($"Members Count: {members?.Count}");
                Console.WriteLine($"Available Books Count: {books?.Count}");

                ViewBag.Members = new SelectList(members ?? new List<Member>(), "Id", "Name");
                //            ViewBag.Books = new SelectList(books ?? new List<Book>(), "BookId", "Title");

                ViewBag.Books = new SelectList(await _bookServiceInterface.GetAvailableBooksByLibraryAsync(libraryId), "BookId", "Title");

                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBorrow(Borrow borrow)
        {
            Console.WriteLine($"MemberId: {borrow.MemberId}, BookId: {borrow.BookId}");

            // ✅ Member ki LibraryId fetch karo
            // var libraryId = await _libraryInterface.GetLibraryIdByMemberAsync(borrow.MemberId);
            var libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;

            Console.WriteLine($"LibraryId: {libraryId}");

            // ❌ Agar Member kisi library ka part nahi hai
            if (libraryId == 0)
            {
                TempData["Error"] = "Library not found! Please select a valid library.";
                return RedirectToAction("AddBorrow");
            }

            //if (!libraryId.HasValue)
            //{
            //    TempData["Error"] = "Member is not associated with any library!";
            //    return RedirectToAction("AddBorrow");
            //}

            // ✅ Validation: Borrow object check karo
            if (borrow == null || borrow.MemberId == 0 || borrow.BookId == 0 || borrow.IssueDate == default || borrow.DueDate == default || borrow.Status == "Borrowed")
            {
                TempData["Error"] = "Invalid data!";
                return RedirectToAction("AddBorrow");
            }

            // ✅ BorrowBookAsync method me LibraryId pass karo
            bool isBorrowed = await _borrowInterface.BorrowBookAsync(borrow.MemberId, borrow.BookId, libraryId, borrow.IssueDate, borrow.DueDate);

            if (isBorrowed)
            {
                TempData["Success"] = "Book borrowed successfully!";
                return RedirectToAction("BorrowList");
            }
            else
            {
                TempData["Error"] = "Failed to borrow book!";
                return RedirectToAction("AddBorrow");
            }
        }




        //[HttpPost]
        //public async Task<IActionResult> AddBorrow(Borrow borrow)
        //{
        //    if (borrow == null || borrow.MemberId == 0 || borrow.BookId == 0 || borrow.IssueDate == default || borrow.DueDate == default)
        //    {
        //        TempData["Error"] = "Invalid data!";
        //        return RedirectToAction("AddBorrow");
        //    }

        //    bool isBorrowed = await _borrowInterface.BorrowBookAsync(borrow.MemberId, borrow.BookId ,borrow.IssueDate,borrow.DueDate);

        //    if (isBorrowed)
        //    {
        //        TempData["Success"] = "Book borrowed successfully!";
        //        return RedirectToAction("BorrowList");
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Failed to borrow book!";
        //        return RedirectToAction("AddBorrow");
        //    }

        //}

        // GET method - Display Return Page


        [HttpGet]
        public async Task<IActionResult> Return()
        {
            string permissionType = GetUserPermission("Borrow Managemet");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                var borrowList = await _borrowInterface.GetAllBorrowsAsync(HttpContext.Session.GetInt32("LibraryId") ?? 0); // List return karo
                return View(borrowList);  // IEnumerable<Borrow> pass kar rahe hai
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }


        [HttpPost]
        public async Task<IActionResult> ReturnBook(int borrowId)
        {
            var borrowRecord = await _connect.Borrows.Include(b => b.Library).FirstOrDefaultAsync(b => b.BorrowId == borrowId);
            if (borrowRecord == null)
                return NotFound();

            borrowRecord.IsReturned = true;
            borrowRecord.Status = "Returned"; // ✅ Explicitly setting status
            borrowRecord.ReturnDate = DateTime.Now;

            int overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;
            decimal fineAmount = (overdueDays > 0) ? (overdueDays * (borrowRecord.Library?.LibraryFineAmount ?? 0)) : 0;

            var existingFine = await _connect.Fines.FirstOrDefaultAsync(f => f.BorrowId == borrowId);
            if (existingFine == null)
            {
                // ✅ Agar fine entry nahi hai, to insert karo
                var fine = new Fine
                {
                    BorrowId = borrowId,
                    FineAmount = fineAmount,
                    PaidAmount = 0,
                    PaymentStatus = (fineAmount == 0) ? "Paid" : "Pending",
                    LibraryId = borrowRecord.LibraryId
                };
                _connect.Fines.Add(fine);
            }
            else
            {
                // ✅ Agar fine entry pehle se hai, to update karo
                existingFine.FineAmount = fineAmount;
                existingFine.PaymentStatus = (fineAmount == 0) ? "Paid" : "Pending";
            }

            int result = await _connect.SaveChangesAsync();
            Console.WriteLine($"SaveChanges Result: {result}");


            TempData["Success"] = "Book returned successfully!";
            return RedirectToAction("BorrowList");
        }


        // POST method - Process Return Action
        //public async Task<IActionResult> ReturnBook(int borrowId)
        //{
        //    var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(borrowId);

        //    if (borrow == null)
        //    {
        //        TempData["Error"] = "Borrow record not found!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    // Calculate fine if the book is returned late
        //    var dueDate = borrow.DueDate;
        //    var returnDate = DateTime.Now;

        //    if (returnDate > dueDate) // If book is returned late
        //    {
        //        var daysLate = (returnDate - dueDate).Days;
        //        var fineAmount = daysLate * 10; // Fine calculation logic (example: ₹10 per day)

        //        // Update fine record
        //        var fine = await _fineInterface.GetFineByBorrowIdAsync(borrowId);
        //        if (fine == null)
        //        {
        //            fine = new Fine
        //            {
        //                BorrowId = borrowId,
        //                FineAmount = fineAmount,
        //                PaidAmount = 0,
        //                PaymentStatus = "Pending",
        //                PaymentDate = null
        //            };
        //            await _fineInterface.AddFineAsync(fine);
        //        }
        //        else
        //        {
        //            fine.FineAmount += fineAmount; // Add fine if already exists
        //            await _fineInterface.UpdateFineAsync(fine);
        //        }
        //    }

        //    // Update return status and return date
        //    borrow.IsReturned = true;
        //    borrow.ReturnDate = returnDate;
        //    await _borrowInterface.UpdateBorrowRecordAsync(borrow);

        //    TempData["Success"] = "Book returned successfully!";
        //    return RedirectToAction("BorrowList", "BorrowMaster");
        //}

        [HttpGet]

        public async Task<IActionResult> UpdateLibraryFine()
        {

            string permissionType = GetUserPermission("Borrow Managemet");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0; // Logged-in admin library
                if (libraryId == 0) return NotFound("Library ID not found in session.");

                var library = await _connect.Libraries.FindAsync(libraryId);
                if (library == null) return NotFound("Library not found.");

                return View(library);

            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
                    }

        [HttpPost]
        public async Task<IActionResult> UpdateLibraryFine(decimal fineAmount)
        {
            int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0;
            if (libraryId == 0) return NotFound("Library ID not found in session.");

            var library = await _connect.Libraries.FindAsync(libraryId);
            if (library == null) return NotFound("Library not found.");

            library.LibraryFineAmount = fineAmount;
            await _connect.SaveChangesAsync();

            TempData["Success"] = "Fine updated successfully!";
            return RedirectToAction("BorrowList");
        }

        [HttpGet]
        public IActionResult PendingBorrows()
        {
            int libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0; // Admin ki Library ID

            if (libraryId == 0)
            {
                return Unauthorized();
            }

            // Pending borrow requests jo approve nahi hui
            var pendingBorrows = _connect.Borrows
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Where(b => b.LibraryId == libraryId && b.Status == "Pending Request")
                .ToList();

            return View(pendingBorrows);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveBorrow([FromBody] Borrow borrow)
        {
            if (borrow?.BorrowId <= 0)
            {
                return Json(new { success = false, message = "Invalid borrow ID!" });
            }

            var borrowRecord = await _connect.Borrows.FindAsync(borrow.BorrowId);
            if (borrowRecord == null)
            {
                return Json(new { success = false, message = "Borrow request not found!" });
            }

            borrowRecord.Status = "Borrowed";
            await _connect.SaveChangesAsync();
            var borrower = await _connect.Members.FindAsync(borrowRecord.MemberId);
            var book = await _connect.Books.FindAsync(borrowRecord.BookId);
            string bookTitle = book != null ? book.Title : "Unknown Book"; // Agar book na mile to "Unknown Book"

            if (borrower != null)
            {
                string subject = "Your Borrow Request is Approved";
                string body = $"Dear {borrower.Name},<br><br>Your request to borrow the book <b>{bookTitle}</b> has been approved.<br><br>Please visit the library to collect your book.<br><br>Regards,<br>Library Team";


                await _emailSenderInterface.SendEmailAsync(borrower.Email, subject, body);
            }

            return Json(new { success = true, message = "Borrow request approved successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBorrow([FromBody] Borrow borrow)
        {
            if (borrow?.BorrowId <= 0)
            {
                return Json(new { success = false, message = "Invalid borrow ID!" });
            }

            var borrowRecord = await _connect.Borrows.FindAsync(borrow.BorrowId);
            if (borrowRecord == null)
            {
                return Json(new { success = false, message = "Borrow request not found!" });
            }

            _connect.Borrows.Remove(borrowRecord);
            await _connect.SaveChangesAsync();
            var borrower = await _connect.Members.FindAsync(borrowRecord.MemberId);
            var book = await _connect.Books.FindAsync(borrowRecord.BookId);
            string bookTitle = book != null ? book.Title : "Unknown Book";

            if (borrower != null)
            {
                string subject = "Your Borrow Request is Rejected";
                string body = $"Dear {borrower.Name},<br><br>Unfortunately, your request to borrow the book <b>{bookTitle}</b> has been rejected.<br><br>Please contact the library for further details.<br><br>Regards,<br>Library Team";

                await _emailSenderInterface.SendEmailAsync(borrower.Email, subject, body);
            }

            return Json(new { success = true, message = "Borrow request rejected successfully!" });
        }


        [HttpGet]
        public IActionResult PendingReturns()
        {
            var libraryId = HttpContext.Session.GetInt32("LibraryId") ?? 0; // Get admin's library

            var pendingReturns = _connect.Borrows
                .Where(b => b.LibraryId == libraryId && b.Status == "Pending Return")
                .Include(b => b.Book)
                .Include(b => b.Member) // Ensure member data is available
                .ToList();

            return View(pendingReturns);
        }

        [HttpPost]
        public IActionResult ApproveReturn(int borrowId)
        {
            var borrow = _connect.Borrows
                .Include(b => b.Book)
                .Include(b => b.Library) // Required for fine calculation
                .FirstOrDefault(b => b.BorrowId == borrowId && b.Status == "Pending Return");

            if (borrow == null)
            {
                TempData["Error"] = "Invalid return request.";
                return RedirectToAction("PendingReturns");
            }

            borrow.ReturnDate = DateTime.Now;

            // Fine Calculation Logic
            decimal fineAmount = 0;
            var finePerDay = _connect.Libraries
                .Where(l => l.LibraryId == borrow.LibraryId)
                .Select(l => l.LibraryFineAmount)
                .FirstOrDefault();

            if (borrow.ReturnDate > borrow.DueDate) // If returned late
            {
                int overdueDays = Math.Max(0, (DateTime.Now - borrow.DueDate).Days);
                fineAmount = (overdueDays > 0) ? (overdueDays * (borrow.Library?.LibraryFineAmount ?? 0)) : 0;
            }

            // Update Borrow Entry
            borrow.IsReturned = true;
            borrow.Status = "Returned";
            _connect.SaveChanges();

            // Check if Fine already exists
            var existingFine = _connect.Fines.FirstOrDefault(f => f.BorrowId == borrow.BorrowId);

            if (fineAmount > 0) // Fine tabhi insert/update hoga jab overdue hai
            {
                if (existingFine == null) // Only insert if no existing fine record
                {
                    var fine = new Fine
                    {
                        BorrowId = borrow.BorrowId,
                        FineAmount = fineAmount,
                        PaidAmount = 0,
                        PaymentStatus = "Pending"
                    };

                    _connect.Fines.Add(fine);
                }
                else
                {
                    // Agar fine record pehle se hai, to sirf amount update kare
                    existingFine.FineAmount = fineAmount;
                    existingFine.PaymentStatus = "Pending";
                }
                _connect.SaveChanges();
            }

            // Update Stock
            var libraryBook = _connect.LibraryBooks
                .FirstOrDefault(lb => lb.LibraryId == borrow.LibraryId && lb.BookId == borrow.BookId);
            if (libraryBook != null)
            {
                libraryBook.Stock += 1;
                _connect.SaveChanges();
            }

            TempData["Success"] = "Book returned successfully.";
            return RedirectToAction("BorrowList");
        }


    }

}

