using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace library_management.repository.classes
{
    public class FineClass : FineInterface
    {
        private readonly dbConnect _context;

        public FineClass(dbConnect connect)
        {
            _context = connect;
        }
        public async Task<Fine> GetFineByIdAsync(int fineId)
        {
            return await _context.Fines
        .Include(f => f.Borrow)
        .ThenInclude(b => b.Member)
        .FirstOrDefaultAsync(f => f.FineId == fineId);
            //return await _context.Fines
            //                .Include(f => f.Borrow)
            //                .FirstOrDefaultAsync(f => f.FineId == fineId);
        }
        public async Task<Fine> GetFineByBorrowIdAsync(int borrowId)
        {
            Debug.WriteLine($"Fetching Fine for BorrowId: {borrowId}");

            var fine = await _context.Fines
                                     .Include(f => f.Borrow)
                                     .FirstOrDefaultAsync(f => f.BorrowId == borrowId);

            Debug.WriteLine(fine != null ? "Fine found!" : "Fine NOT found!");

            return fine;
        }

        public async Task<bool> AddFineAsync(Fine fine)
        {
            if (fine == null)
                return false;

            await _context.Fines.AddAsync(fine);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFineAsync(Fine fine)
        {
            var existingFine = await _context.Fines.FindAsync(fine.FineId);
            if (existingFine == null)
                return false;

            existingFine.PaidAmount = fine.PaidAmount;
            existingFine.PaymentStatus = fine.PaymentStatus;
            existingFine.PaymentDate = fine.PaymentDate;

            await _context.SaveChangesAsync();
            return true;
        }


        //public async Task<decimal> CalculateFineAsync(int borrowId)
        //{
        //    var borrowRecord = await _context.Borrows.FindAsync(borrowId);
        //    if (borrowRecord == null) return 0;

        //    int overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;
        //    if (overdueDays > 0)
        //    {
        //        var finePolicy = await _context.FinePolicies
        //            .FirstOrDefaultAsync(p => p.LibraryId == borrowRecord.LibraryId);

        //        decimal finePerDay = finePolicy?.FinePerDay ?? 0;
        //        decimal fineAmount = overdueDays * finePerDay;

        //        if (finePolicy?.MaxFineAmount != null)
        //        {
        //            fineAmount = Math.Min(fineAmount, finePolicy.MaxFineAmount.Value);
        //        }

        //        return fineAmount;
        //    }

        //    return 0;
        //}

        public async Task<decimal> CalculateFineAsync(int borrowId)
        {
            var borrowRecord = await _context.Borrows.Include(b => b.Library).FirstOrDefaultAsync(b => b.BorrowId == borrowId);
            if (borrowRecord == null || borrowRecord.DueDate == null)
                return 0;

            int overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;

            if (overdueDays <= 0) return 0; // Agar overdue nahi hai, to fine nahi lagega

            var libraryFineAmount = borrowRecord.Library?.LibraryFineAmount ?? 0; // ✅ Fine amount jo librarian ne set kiya hai

            decimal fineAmount = overdueDays * libraryFineAmount;

            return fineAmount;
        }

        public byte[] GenerateFineReceiptPdf(Fine fine)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                // Centered Title
                Paragraph title = new Paragraph("Library Fine Receipt", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var borrow = _context.Borrows.FirstOrDefault(b => b.BorrowId == fine.BorrowId);
                var book = _context.Books.FirstOrDefault(b => b.BookId == borrow.BookId);

                // Safe null handling
                string memberName = fine?.Borrow?.Member?.Name ?? "N/A";
                string bookTitle = book?.Title ?? "Unknown";

                // Receipt Details - Center aligned table
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 80;
                table.HorizontalAlignment = Element.ALIGN_CENTER;
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;

                // Helper method to add a row
                void AddRow(string label, string value)
                {
                    PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
                    PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));

                    labelCell.Border = Rectangle.NO_BORDER;
                    valueCell.Border = Rectangle.NO_BORDER;

                    table.AddCell(labelCell);
                    table.AddCell(valueCell);
                }

                AddRow("Member Name:", memberName);
                AddRow("Book Title:", bookTitle);
                AddRow("Fine Amount:", $"₹{fine.FineAmount}");
                AddRow("Paid Amount:", $"₹{fine.PaidAmount}");
                AddRow("Payment Status:", fine.PaymentStatus);
                AddRow("Date:", DateTime.Now.ToString("dd-MM-yyyy HH:mm"));

                document.Add(table);

                // Footer
                Paragraph footer = new Paragraph("Thank you for using our Library!", valueFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 20f;
                document.Add(footer);

                document.Close();
                return stream.ToArray();
            }
        }


        public async Task AddTransactionAsync(TblTransaction transaction)
        {
            _context.TblTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }



        //public async Task<decimal> CalculateFineAsync(int borrowId)
        //{
        //    var borrowRecord = await _context.Borrows.FindAsync(borrowId);

        //    // NULL CHECK: Borrow record exist karta hai ya nahi
        //    if (borrowRecord == null)
        //    {
        //        Console.WriteLine("Borrow record not found!");
        //        return 0;
        //    }

        //    // Calculate overdue days safely
        //    int overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;

        //    // Ensure overdue days are not negative
        //    if (overdueDays <= 0)
        //        return 0;

        //    // Get the fine record for this borrow (if exists)
        //    var fineRecord = await _context.Fines
        //        .FirstOrDefaultAsync(f => f.BorrowId == borrowId);

        //    // NULL CHECK: Fine record nahi mila toh 0 return karo
        //    if (fineRecord == null)
        //    {
        //        Console.WriteLine("Fine record not found!");
        //        return 0;
        //    }

        //    decimal finePerDay = fineRecord.FineAmount; // NULL Handling
        //    decimal fineAmount = overdueDays * finePerDay;

        //    return fineAmount;
        //}


        //public async Task<decimal> CalculateFineAsync(int borrowId)
        //{
        //    var borrowRecord = await _context.Borrows.FindAsync(borrowId);
        //    if (borrowRecord == null)
        //        return 0;

        //    int overdueDays = (DateTime.Now - borrowRecord.DueDate).Days;

        //    // Ensure overdue days are not negative
        //    if (overdueDays <= 0)
        //        return 0;

        //    // Get the last fine record for this borrow (if exists)
        //    var fineRecord = await _context.Fines
        //        .FirstOrDefaultAsync(f => f.BorrowId == borrowId);

        //    if (fineRecord == null)
        //        return 0; // No fine record found, meaning fine wasn't applied

        //    decimal finePerDay = fineRecord.FineAmount;
        //    decimal fineAmount = overdueDays * finePerDay;

        //    return fineAmount;
        //}
    }
}
