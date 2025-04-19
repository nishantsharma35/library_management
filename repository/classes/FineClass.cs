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
                            .FirstOrDefaultAsync(f => f.FineId == fineId);
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
                Document document = new Document();
                PdfWriter.GetInstance(document, stream);
                document.Open();

                document.Add(new Paragraph("Library Fine Receipt"));
                document.Add(new Paragraph($"Fine ID: {fine.FineId}"));
                document.Add(new Paragraph($"Borrow ID: {fine.BorrowId}"));
                document.Add(new Paragraph($"Amount: ₹{fine.FineAmount}"));
                document.Add(new Paragraph($"Paid Amount: ₹{fine.PaidAmount}"));
                document.Add(new Paragraph($"Payment Status: {fine.PaymentStatus}"));
                document.Add(new Paragraph($"Date: {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}"));

                document.Close();
                return stream.ToArray(); // 📎 Convert PDF to Byte Array
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
