using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using library_management.repository.classes;

namespace library_management.Controllers
{
    public class FineController : BaseController
    {
        private readonly dbConnect _context;
        private readonly FineInterface _fineInterface;
        private readonly BorrowInterface _borrowInterface;
        private readonly EmailSenderInterface _emailService;
        private readonly IActivityRepository _activityRepository;
        private readonly PermisionHelperInterface _permission;

        public FineController(
    dbConnect connect,
    FineInterface fineInterface,
    BorrowInterface borrowInterface,
    ISidebarRepository sidebar,
    EmailSenderInterface emailSender,
    IActivityRepository activityRepository,
    PermisionHelperInterface permission) : base(sidebar)
        {
            _borrowInterface = borrowInterface;
            _context = connect;
            _fineInterface = fineInterface;
            _emailService = emailSender;
            _activityRepository = activityRepository;
            _permission = permission;
        }


        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ViewFine(int borrowId)
        {
            // Get the fine for the borrowId
            Debug.WriteLine($"Received BorrowId: {borrowId}");

            var fine = await _fineInterface.GetFineByBorrowIdAsync(borrowId);

            if (fine == null)
            {
                TempData["Error"] = $"No fine found for this borrow record! BorrowId: {borrowId}";
                return RedirectToAction("BorrowList", "BorrowMaster");
            }

            // ✅ If Fine is Paid, Generate PDF Receipt
            if (fine.PaymentStatus == "Paid")
            {
                return GenerateFineReceipt(fine);
            }

            return View(fine); // Normal View for unpaid fines
        }

        private IActionResult GenerateFineReceipt(Fine fine)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // 🔠 Fonts
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

                // 🔄 Fetch data
                var borrow = _context.Borrows.FirstOrDefault(b => b.BorrowId == fine.BorrowId);
                var member = _context.Members.FirstOrDefault(m => m.Id == borrow.MemberId);
                var book = _context.Books.FirstOrDefault(b => b.BookId == borrow.BookId);
                var library = _context.Libraries.FirstOrDefault(l => l.LibraryId == borrow.LibraryId);

                string memberName = member?.Name ?? "Unknown";
                string bookTitle = book?.Title ?? "Unknown";
                string libraryName = library?.Libraryname ?? "Unknown Library";

                // 🏛 Title
                Paragraph title = new Paragraph($"{libraryName} - Fine Receipt", headerFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                // 📋 Table setup
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 70;
                table.HorizontalAlignment = Element.ALIGN_CENTER;
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;

                float[] columnWidths = { 1.5f, 2f };
                table.SetWidths(columnWidths);

                // Utility to add centered cells
                void AddCenteredCell(string text, Font font)
                {
                    var cell = new PdfPCell(new Phrase(text, font))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        PaddingBottom = 8f
                    };
                    table.AddCell(cell);
                }

                // Add data rows
                AddCenteredCell("Member Name", labelFont);
                AddCenteredCell(memberName, valueFont);

                AddCenteredCell("Book Title", labelFont);
                AddCenteredCell(bookTitle, valueFont);

                AddCenteredCell("Borrow Date", labelFont);
                AddCenteredCell(borrow?.IssueDate.ToString("dd-MM-yyyy") ?? "-", valueFont);

                AddCenteredCell("Amount", labelFont);
                AddCenteredCell($"₹{fine.FineAmount}", valueFont);

                AddCenteredCell("Paid Amount", labelFont);
                AddCenteredCell($"₹{fine.PaidAmount}", valueFont);

                AddCenteredCell("Payment Status", labelFont);
                AddCenteredCell(fine.PaymentStatus, valueFont);

                AddCenteredCell("Receipt Date", labelFont);
                AddCenteredCell(DateTime.Now.ToString("dd-MM-yyyy HH:mm"), valueFont);

                document.Add(table);

                // 🧾 Footer
                Paragraph footer = new Paragraph("Thank you for using PaperByte Library System.", valueFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20f
                };
                document.Add(footer);

                document.Close();
                writer.Close();

                byte[] fileBytes = stream.ToArray();
                if(fileBytes != null)
                {
                    string type = "download fine receipt";
                    string desc = $"{userName} downloaded fine receipt from {libName}";
                    _activityRepository.AddNewActivity(id, type, desc);
                }

                return File(fileBytes, "application/pdf", $"Fine_Receipt_{memberName.Replace(" ", "_")}_{fine.FineId}.pdf");
            }
        }






        [HttpGet]
        public async Task<IActionResult> AddFine(int borrowId)
        {
            var borrowRecord = await _borrowInterface.GetBorrowRecordByIdAsync(borrowId);

            if (borrowRecord == null)
            {
                TempData["Error"] = "Invalid borrow record!";
                return RedirectToAction("BorrowList", "BorrowMaster");
            }

            // Calculate Fine Logic (this should be based on overdue days and your fine rules)
            decimal fineAmount = CalculateFine(borrowRecord); // Create a method to calculate fine

            var fine = new Fine
            {
                BorrowId = borrowRecord.BorrowId,
                FineAmount = fineAmount,
                PaymentStatus = "Pending"
            };

            await _fineInterface.AddFineAsync(fine); // Add fine to the database

            TempData["Success"] = "Fine added successfully!";
            return RedirectToAction("BorrowList", "BorrowMaster");
        }

        [HttpPost]
        public async Task<IActionResult> PayFine(int fineId, decimal payAmount)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var fine = await _fineInterface.GetFineByIdAsync(fineId);
            var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(fine.BorrowId);
            var member = borrow?.Member;

            if (fine == null)
            {
                return Json(new { success = false, message = "Fine not found!" });
            }

            decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
            if (payAmount > remainingAmount)
            {
                return Json(new { success = false, message = "Payment amount exceeds remaining fine!" });
            }

            fine.PaidAmount += payAmount;
            if (fine.PaidAmount >= fine.FineAmount)
            {
                fine.PaymentStatus = "Paid";
            }
            fine.PaymentDate = DateTime.Now;

            var transaction = new TblTransaction
            {
                FineId = fine.FineId,
                AmountPaid = (int)payAmount,
                PaymentDate = DateTime.Now,
                PaymentMode = "Cash",
                Reference = "Manual Payment"
            };

            await _fineInterface.AddTransactionAsync(transaction);
            await _fineInterface.UpdateFineAsync(fine);

            string subject = "Fine Payment Confirmation";
            string memberName = fine.Borrow?.Member?.Name ?? "Guest"; // Default to "Guest"
            string body = fine.Borrow?.Member != null
                ? $"Dear {fine.Borrow.Member.Name},\n\n" +
                  $"You have made a payment of ₹{payAmount} towards your fine.\n\n" +
                  $"Remaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\n\n"
                : "Dear Member,\n\nYour fine has been updated.";

            if (fine.PaymentStatus == "Paid")
            {
                body += "Your fine has been fully paid. Thank you for using our service!";
            }
            else
            {
                body += "Please pay the remaining amount to fully clear your fine.";
            }

            string recipientEmail = fine.Borrow?.Member?.Email ?? "defaultEmail@example.com";

            byte[] pdfBytes = _fineInterface.GenerateFineReceiptPdf(fine);
            await _emailService.SendEmailWithAttachment(recipientEmail, subject, body, pdfBytes, $"Fine_Receipt_{fine.FineId}.pdf");

            string type = "Pay Fine";
            string desc = $"{userName} manually paid fine in cash at {libName}";
            _activityRepository.AddNewActivity(id, type, desc);

            return Json(new { success = true, message = "Fine payment and transaction recorded successfully!" });
        }

        //[HttpPost]
        //public async Task<IActionResult> PayFine(int fineId, decimal payAmount)
        //{
        //    var fine = await _fineInterface.GetFineByIdAsync(fineId);
        //    if (fine == null)
        //    {
        //        TempData["Error"] = "Fine not found!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    decimal remainingAmount = fine.FineAmount - fine.PaidAmount;

        //    // Validate Payment Amount
        //    if (payAmount <= 0 || payAmount > remainingAmount)
        //    {
        //        TempData["Error"] = $"Invalid amount! You can pay between ₹1 and ₹{remainingAmount}.";
        //        return RedirectToAction("ViewFine", new { borrowId = fine.BorrowId });
        //    }

        //    // Update Paid Amount
        //    fine.PaidAmount += payAmount;

        //    // Update Payment Status
        //    fine.PaymentStatus = fine.PaidAmount >= fine.FineAmount ? "Paid" : "Partially Paid";
        //    fine.PaymentDate = DateTime.Now;

        //    await _fineInterface.UpdateFineAsync(fine);

        //    TempData["Success"] = "Payment successful!";
        //    return RedirectToAction("ViewFine", new { borrowId = fine.BorrowId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> PayFine([FromBody] Fine fineData)
        //{
        //    var fine = await _fineInterface.GetFineByIdAsync(fineData.FineId);
        //    var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(fine.BorrowId); // Ensure Borrow and Member are loaded
        //    var member = borrow?.Member; // Ensure Member is not null

        //    // If member is still null, log an error or handle accordingly

        //    if (fine == null)
        //    {
        //        TempData["Error"] = "Fine not found!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    // Check if the payment amount is greater than the remaining fine
        //    decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
        //    if (fineData.FineAmount > remainingAmount)
        //    {
        //        TempData["Error"] = "Payment amount exceeds remaining fine!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    // Update paid amount and payment status based on the partial payment
        //    fine.PaidAmount += fineData.FineAmount;
        //    if (fine.PaidAmount >= fine.FineAmount)
        //    {
        //        fine.PaymentStatus = "Paid";
        //    }
        //    fine.PaymentDate = DateTime.Now;

        //    await _fineInterface.UpdateFineAsync(fine); // Update fine in DB

        //    // Send email notification about the partial payment
        //    string subject = "Fine Payment Confirmation";
        //    string memberName = fine.Borrow?.Member?.Name ?? "Guest"; // Default to "Guest" if Member is null

        //    string body = fine.Borrow?.Member != null
        //        ? $"Dear {fine.Borrow.Member.Name},\n\n" +
        //          $"You have made a payment of ₹{fineData.FineAmount} towards your fine.\n\n" +
        //          $"Remaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\n\n"
        //        : "Dear Member,\n\nYour fine has been updated.";

        //    // If fully paid, update email body
        //    if (fine.PaymentStatus == "Paid")
        //    {
        //        body += "Your fine has been fully paid. Thank you for using our service!";
        //    }
        //    else
        //    {
        //        body += "Please pay the remaining amount to fully clear your fine.";
        //    }

        //    string recipientEmail = fine.Borrow?.Member?.Email ?? "defaultEmail@example.com"; // Default email if null

        //    // ✅ Generate Fine Receipt PDF
        //    byte[] pdfBytes =  _fineInterface.GenerateFineReceiptPdf(fine);

        //    // ✅ Send Email with PDF Attachment
        //    await _emailService.SendEmailWithAttachment(recipientEmail, subject, body, pdfBytes, $"Fine_Receipt_{fine.FineId}.pdf");


        //    TempData["Success"] = "Fine payment processed successfully!";
        //    return RedirectToAction("BorrowList", "BorrowMaster");
        //}




        //[HttpPost]
        //public async Task<IActionResult> PayFine([FromBody] Fine fineData)
        //{
        //    var fine = await _fineInterface.GetFineByIdAsync(fineData.FineId);
        //    var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(fine.BorrowId);
        //    var member = borrow?.Member;

        //    if (fine == null)
        //    {
        //        TempData["Error"] = "Fine not found!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
        //    if (fineData.FineAmount > remainingAmount)
        //    {
        //        TempData["Error"] = "Payment amount exceeds remaining fine!";
        //        return RedirectToAction("BorrowList", "BorrowMaster");
        //    }

        //    // ✅ Save Transaction Before Updating Fine
        //    var transaction = new TblTransaction
        //    {
        //        FineId = fine.FineId,
        //        AmountPaid = (int)fineData.FineAmount,
        //        PaymentDate = DateTime.Now,
        //        PaymentMode = "Cash", // Ya aap UI se bhi bhej sakte ho
        //        Reference = "Manual Payment"
        //    };

        //    await _fineInterface.AddTransactionAsync(transaction); // ✅ Add to DB (Make sure interface/service is set)

        //    // ✅ Update fine with paid amount and status
        //    fine.PaidAmount += fineData.FineAmount;
        //    if (fine.PaidAmount >= fine.FineAmount)
        //    {
        //        fine.PaymentStatus = "Paid";
        //    }
        //    fine.PaymentDate = DateTime.Now;

        //    await _fineInterface.UpdateFineAsync(fine);

        //    // ✅ Send Email
        //    string subject = "Fine Payment Confirmation";
        //    string body = member != null
        //        ? $"Dear {member.Name},\n\nYou have made a payment of ₹{fineData.FineAmount} towards your fine.\nRemaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\n\n"
        //        : "Dear Member,\n\nYour fine has been updated.\n\n";

        //    if (fine.PaymentStatus == "Paid")
        //        body += "Your fine has been fully paid. Thank you for using our service!";
        //    else
        //        body += "Please pay the remaining amount to fully clear your fine.";

        //    string recipientEmail = member?.Email ?? "defaultEmail@example.com";
        //    byte[] pdfBytes = _fineInterface.GenerateFineReceiptPdf(fine);
        //    await _emailService.SendEmailWithAttachment(recipientEmail, subject, body, pdfBytes, $"Fine_Receipt_{fine.FineId}.pdf");

        //    TempData["Success"] = "Fine payment and transaction recorded successfully!";
        //    return RedirectToAction("BorrowList", "BorrowMaster");
        //}



        //[HttpPost]
        //public async Task<IActionResult> PayFine(int fineId, decimal payAmount)
        //{
        //   int id = (int)HttpContext.Session.GetInt32("UserId");
        //    string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
        //    string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

        //    var fine = await _fineInterface.GetFineByIdAsync(fineId);
        //    var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(fine.BorrowId); // Ensure Borrow and Member are loaded
        //    var member = borrow?.Member; // Ensure Member is not null

        //    // If member is still null, log an error or handle accordingly

        //    if (fine == null)
        //    {
        //        return Json(new { success = false, message = "Fine not found!" });
        //    }

        //    // Check if the payment amount is greater than the remaining fine
        //    decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
        //    if (payAmount > remainingAmount)
        //    {
        //        return Json(new { success = false, message = "Payment amount exceeds remaining fine!" });
        //    }

        //    // Update paid amount and payment status based on the partial payment
        //    fine.PaidAmount += payAmount;
        //    if (fine.PaidAmount >= fine.FineAmount)
        //    {
        //        fine.PaymentStatus = "Paid";
        //    }
        //    fine.PaymentDate = DateTime.Now;


        //    var transaction = new TblTransaction
        //    {
        //        FineId = fine.FineId,
        //        AmountPaid = (int)payAmount,
        //        PaymentDate = DateTime.Now,
        //        PaymentMode = "Cash", // Ya aap UI se bhi bhej sakte ho
        //        Reference = "Manual Payment"
        //    };

        //    await _fineInterface.AddTransactionAsync(transaction);

        //    await _fineInterface.UpdateFineAsync(fine); // Update fine in DB

        //    // Send email notification about the partial payment
        //    string subject = "Fine Payment Confirmation";
        //    string memberName = fine.Borrow?.Member?.Name ?? "Guest"; // Default to "Guest" if Member is null

        //    string body = fine.Borrow?.Member != null
        //        ? $"Dear {fine.Borrow.Member.Name},\n\n" +
        //          $"You have made a payment of ₹{payAmount} towards your fine.\n\n" +
        //          $"Remaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\n\n"
        //        : "Dear Member,\n\nYour fine has been updated.";

        //    // If fully paid, update email body
        //    if (fine.PaymentStatus == "Paid")
        //    {
        //        body += "Your fine has been fully paid. Thank you for using our service!";
        //    }
        //    else
        //    {
        //        body += "Please pay the remaining amount to fully clear your fine.";
        //    }

        //    string recipientEmail = fine.Borrow?.Member?.Email ?? "defaultEmail@example.com"; // Default email if null

        //    // ✅ Generate Fine Receipt PDF
        //    byte[] pdfBytes = _fineInterface.GenerateFineReceiptPdf(fine);

        //    // ✅ Send Email with PDF Attachment
        //    await _emailService.SendEmailWithAttachment(recipientEmail, subject, body, pdfBytes, $"Fine_Receipt_{fine.FineId}.pdf");

        //    string type = "Pay Fine";
        //    string desc = $"{userName} manually Pay fine in cash to his{libName}";
        //    _activityRepository.AddNewActivity(id, type, desc);

        //    return Json(new { success = true, message = "Fine payment and transaction recorded successfully!" });

        //}


        private decimal CalculateFine(Borrow borrowRecord)
        {
            var overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;
            if (overdueDays > 0)
            {
                return overdueDays * 5.0m; // Assume 5 per day fine
            }
            return 0;
        }


        [HttpGet]
        public async Task<IActionResult> UpdateFine()
        {
            string permissionType = GetUserPermission("Admin Management");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                var libraries = await _context.Libraries.ToListAsync();
            return View(libraries);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateFine(int libraryId, decimal fineAmount)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) return NotFound();

            library.LibraryFineAmount = fineAmount;
            await _context.SaveChangesAsync();

            string type = "updated Fine";
            string desc = $"{userName} updated {libName} library fine";
            _activityRepository.AddNewActivity(id, type, desc);

            return Json(new { success = true, message = "Fine updated successfully!" });
            //TempData["Success"] = "Fine updated successfully!";
            //return RedirectToAction("UpdateFine");
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFineReceipt(int borrowId)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _context.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var fine = await _fineInterface.GetFineByBorrowIdAsync(borrowId);

            if (fine == null || fine.PaymentStatus != "Paid")
            {
                TempData["Error"] = "Fine not found or not paid!";
                return RedirectToAction("ViewFine", new { borrowId });
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("Library Fine Receipt", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                document.Add(new Paragraph("\n"));

                // Fine Details Table
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.AddCell("Borrow ID");
                table.AddCell(fine.BorrowId.ToString());
                table.AddCell("Fine Amount");
                table.AddCell("₹" + fine.FineAmount);
                table.AddCell("Paid Amount");
                table.AddCell("₹" + fine.PaidAmount);
                table.AddCell("Payment Status");
                table.AddCell(fine.PaymentStatus);
                table.AddCell("Payment Date");
                table.AddCell(fine.PaymentDate?.ToString("dd/MM/yyyy"));

                document.Add(table);

                document.Close();
                writer.Close();

                string type = "download Fine receipt";
                string desc = $"{userName}downloaded {libName} library fine receipt";
                _activityRepository.AddNewActivity(id, type, desc);

                return File(ms.ToArray(), "application/pdf", $"Fine_Receipt_{fine.BorrowId}.pdf");
            }
        }




    }
}
