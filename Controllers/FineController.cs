using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;

namespace library_management.Controllers
{
    public class FineController : BaseController
    {
        private readonly dbConnect _context;
        private readonly FineInterface _fineInterface;
        private readonly BorrowInterface _borrowInterface;
        private readonly EmailSenderInterface _emailService;

        public FineController(dbConnect connect, FineInterface fineInterface, BorrowInterface borrowInterface, ISidebarRepository sidebar, EmailSenderInterface emailSender) : base(sidebar)
        {
            _borrowInterface = borrowInterface;
            _context = connect;
            _fineInterface = fineInterface;
            _emailService = emailSender;
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
            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // ✅ Set font style
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

                // ✅ Add Title
                document.Add(new Paragraph("Library Fine Receipt", titleFont));
                document.Add(new Paragraph("\n")); // Line break

                // ✅ Add Fine Details
                document.Add(new Paragraph($"Fine ID: {fine.FineId}", normalFont));
                document.Add(new Paragraph($"Borrow ID: {fine.BorrowId}", normalFont));
                document.Add(new Paragraph($"Amount: ₹{fine.FineAmount}", normalFont));
                document.Add(new Paragraph($"Paid Amount: ₹{fine.PaidAmount}", normalFont));
                document.Add(new Paragraph($"Payment Status: {fine.PaymentStatus}", normalFont));
                document.Add(new Paragraph($"Date: {DateTime.Now:dd-MM-yyyy HH:mm}", normalFont));

                document.Close();
                writer.Close();

                // ✅ Ensure stream is converted before returning
                byte[] fileBytes = stream.ToArray();

                return File(fileBytes, "application/pdf", $"Fine_Receipt_{fine.FineId}.pdf");
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

        [HttpPost]
        public async Task<IActionResult> PayFine(int fineId, decimal payAmount)
        {
            var fine = await _fineInterface.GetFineByIdAsync(fineId);
            var borrow = await _borrowInterface.GetBorrowRecordByIdAsync(fine.BorrowId); // Ensure Borrow and Member are loaded
            var member = borrow?.Member; // Ensure Member is not null

            // If member is still null, log an error or handle accordingly

            if (fine == null)
            {
                TempData["Error"] = "Fine not found!";
                return RedirectToAction("BorrowList", "BorrowMaster");
            }

            // Check if the payment amount is greater than the remaining fine
            decimal remainingAmount = fine.FineAmount - fine.PaidAmount;
            if (payAmount > remainingAmount)
            {
                TempData["Error"] = "Payment amount exceeds remaining fine!";
                return RedirectToAction("BorrowList", "BorrowMaster");
            }

            // Update paid amount and payment status based on the partial payment
            fine.PaidAmount += payAmount;
            if (fine.PaidAmount >= fine.FineAmount)
            {
                fine.PaymentStatus = "Paid";
            }
            fine.PaymentDate = DateTime.Now;

            await _fineInterface.UpdateFineAsync(fine); // Update fine in DB

            // Send email notification about the partial payment
            string subject = "Fine Payment Confirmation";
            string memberName = fine.Borrow?.Member?.Name ?? "Guest"; // Default to "Guest" if Member is null

            string body = fine.Borrow?.Member != null
                ? $"Dear {fine.Borrow.Member.Name},\n\n" +
                  $"You have made a payment of ₹{payAmount} towards your fine.\n\n" +
                  $"Remaining Amount: ₹{fine.FineAmount - fine.PaidAmount}\n\n"
                : "Dear Member,\n\nYour fine has been updated.";

            // If fully paid, update email body
            if (fine.PaymentStatus == "Paid")
            {
                body += "Your fine has been fully paid. Thank you for using our service!";
            }
            else
            {
                body += "Please pay the remaining amount to fully clear your fine.";
            }

            string recipientEmail = fine.Borrow?.Member?.Email ?? "defaultEmail@example.com"; // Default email if null

            // ✅ Generate Fine Receipt PDF
            byte[] pdfBytes =  _fineInterface.GenerateFineReceiptPdf(fine);

            // ✅ Send Email with PDF Attachment
            await _emailService.SendEmailWithAttachment(recipientEmail, subject, body, pdfBytes, $"Fine_Receipt_{fine.FineId}.pdf");


            TempData["Success"] = "Fine payment processed successfully!";
            return RedirectToAction("BorrowList", "BorrowMaster");
        }




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
            var libraries = await _context.Libraries.ToListAsync();
            return View(libraries);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateFine(int libraryId, decimal fineAmount)
        {
            var library = await _context.Libraries.FindAsync(libraryId);
            if (library == null) return NotFound();

            library.LibraryFineAmount = fineAmount;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fine updated successfully!";
            return RedirectToAction("UpdateFine");
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFineReceipt(int borrowId)
        {
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

                return File(ms.ToArray(), "application/pdf", $"Fine_Receipt_{fine.BorrowId}.pdf");
            }
        }




    }
}
