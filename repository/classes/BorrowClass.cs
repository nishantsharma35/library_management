using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace library_management.repository.classes
{
    public class BorrowClass : BorrowInterface
    {
        private readonly dbConnect _connect;
        private readonly EmailSenderInterface _emailSender;

        public BorrowClass(dbConnect connect, EmailSenderInterface emailSenderInterface)
        {
            _connect = connect;
            _emailSender = emailSenderInterface;
        }


        public async Task<int> BorrowBookAsync(int memberId, int bookId, int libraryId, DateTime issueDate, DateTime dueDate)
        {
            Console.WriteLine($"LOG: Borrowing Book - Member {memberId}, Book {bookId}, Library {libraryId}");

            string email = _connect.Members.Where(x=>x.Id== memberId).Select(y=>y.Email).FirstOrDefault();
            var alreadyBorrowed = await _connect.Borrows
                .AnyAsync(b => b.MemberId == memberId
                            && b.BookId == bookId
                            && b.LibraryId == libraryId
                            && b.ReturnDate == null);

            if (alreadyBorrowed)
            {
                Console.WriteLine($"❌ ERROR: Member {memberId} has already borrowed Book {bookId} from Library {libraryId}");
                return 0;
            }
            var otp = _emailSender.GenerateOtp();
            var otpexpiry = DateTime.Now.AddMinutes(5);

            var borrow = new Borrow
            {
                MemberId = memberId,
                BookId = bookId,
                LibraryId = libraryId,
                IssueDate = issueDate,
                DueDate = dueDate,
                Status = "Pending",
                ReturnDate = null,
                otp = otp,
                otpexpires = otpexpiry
            };
            string subject = "Your Borrow Request OTP Verification";
            string body = $"Dear {borrow.MemberId},<br><br>Your request to borrow the book <b>{bookId}</b> has been received.<br><br>To complete the process, please use the following OTP to verify your request:<br><br><b>{otp}</b><br><br>This OTP will expire in 5 minutes.<br><br>Regards,<br>Library Team";

            await _emailSender.SendEmailAsync(email, subject, body);

            _connect.Borrows.Add(borrow);
            await _connect.SaveChangesAsync();
            return borrow.BorrowId;;
        }

        public async Task<bool> VerifyOtpAsync(int borrowId, string enteredOtp)
        {
            var borrowRecord = await _connect.Borrows
                .Where(b => b.BorrowId == borrowId)
                .FirstOrDefaultAsync();

            if (borrowRecord == null)
            {
                Console.WriteLine("❌ ERROR: Borrow record not found.");
                return false;
            }

            if (borrowRecord.otp != enteredOtp)
            {
                Console.WriteLine("❌ ERROR: OTP does not match.");
                return false;
            }

            if (borrowRecord.otpexpires < DateTime.Now)
            {
                Console.WriteLine("❌ ERROR: OTP has expired.");
                return false;
            }

            borrowRecord.Status = "Borrowed";
            _connect.Borrows.Update(borrowRecord);
            await _connect.SaveChangesAsync();
            // OTP verification successful
            return true;
        }




        //public async Task<bool> BorrowBookAsync(int memberId, int bookId, int libraryId, DateTime issueDate, DateTime dueDate)
        //{
        //    var borrow = new Borrow
        //    {
        //        MemberId = memberId,
        //        BookId = bookId,
        //        LibraryId = libraryId, // ✅ LibraryId assign karo
        //        IssueDate = issueDate,
        //        DueDate = dueDate,
        //        Status = "Borrowed"
        //    };

        //    _connect.Borrows.Add(borrow);
        //    await _connect.SaveChangesAsync();
        //    return true;
        //}


        public async Task<List<Borrow>> GetAllBorrowsAsync(int libraryId)
        {
            return await _connect.Borrows
       .Include(b => b.Book)
       .Include(b => b.Member)
       .Include(b => b.Fine)
       .Where(b => _connect.Memberships
           .Any(m => m.MemberId == b.MemberId && m.LibraryId == libraryId)) // ✅ Library filter yaha lagaya
       .ToListAsync();

            //        return await _connect.Borrows
            //.Include(b => b.Book)
            //.Include(b => b.Member)
            //.Include(b => b.Fine)
            //.Where(b => b.Book.LibraryId == libraryId) // ✅ Correct filter: Book's Library
            //.ToListAsync();


            //    var borrows=  await _connect.Borrows
            //.Where(b => b.LibraryId == libraryId) // ✅ Correct filter
            //.Include(b => b.Member)
            //.Include(b => b.Book)
            //.ToListAsync();
            //     return borrows;
            //   var borrows = await _connect.Borrows
            //.Include(b => b.Book)
            //.ThenInclude(book => book.Genre) // Agar Book me aur relations ho
            //.Include(b => b.Member)
            //.Include(b => b.Library).Where(l=> l.LibraryId == libraryId)  // ✅ Library Include karo
            //.ToListAsync();

            //foreach (var b in borrows)
            //{
            //    Console.WriteLine($"BorrowId: {b.BorrowId}, BookId: {b.Book?.BookId}, MemberId: {b.Member?.Id}, LibraryId: {b.LibraryId}");
            //}


            //var borrows = await _connect.Borrows
            //     .Include(b => b.Member)
            //     .Include(b => b.Book)
            //     .ToListAsync();

            //Console.WriteLine($"Fetched {borrows.Count} borrow records");
            //return borrows;
        }


        public async Task<bool> ReturnBookAsync(int borrowId)
        {
            var borrowRecord = await _connect.Borrows
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrowRecord == null || borrowRecord.IsReturned)
                return false;

            // ✅ Member ke through LibraryId fetch karo
            var libraryId = await _connect.Members
                .Where(m => m.Id == borrowRecord.MemberId)
                .Select(m => m.Memberships.Select(ms => ms.LibraryId).FirstOrDefault())  // ✅ Membership table ka use karo
                .FirstOrDefaultAsync();

            if (libraryId == 0)
                return false;

            borrowRecord.IsReturned = true;
            borrowRecord.ReturnDate = DateTime.Now;

            // ✅ LibraryBooks ka stock update karo
            var libraryBook = await _connect.LibraryBooks
                .FirstOrDefaultAsync(lb => lb.BookId == borrowRecord.BookId && lb.LibraryId == libraryId);

            if (libraryBook != null)
            {
                libraryBook.Stock++; // ✅ Increase stock properly
            }

            // ✅ Fine Calculation - Late return check
            if (borrowRecord.ReturnDate > borrowRecord.DueDate)
            {
                int daysLate = (borrowRecord.ReturnDate.Value - borrowRecord.DueDate).Days;
                decimal fineAmount = daysLate * 10; // ₹10 per day fine

                var fine = await _connect.Fines.FirstOrDefaultAsync(f => f.BorrowId == borrowId);
                if (fine == null)
                {
                    fine = new Fine
                    {
                        BorrowId = borrowId,
                        FineAmount = fineAmount,
                        PaidAmount = 0,
                        PaymentStatus = "Pending",
                        PaymentDate = null
                    };
                    await _connect.Fines.AddAsync(fine);
                }
                else if (fine.PaidAmount < fine.FineAmount)
                {
                    fine.FineAmount += fineAmount; // ✅ Only update if unpaid fine exists
                }
            }

            await _connect.SaveChangesAsync();
            return true;
        }


        //public async Task<bool> ReturnBookAsync(int borrowId)
        //{
        //    var borrowRecord = await _connect.Borrows.FindAsync(borrowId);
        //    if (borrowRecord == null || borrowRecord.IsReturned)
        //        return false;

        //    borrowRecord.IsReturned = true;
        //    borrowRecord.ReturnDate = DateTime.Now;

        //    var book = await _connect.Books.FindAsync(borrowRecord.BookId);
        //    if (book != null)
        //    {
        //        // Find the corresponding LibraryBooks entry based on the BookId and the LibraryId of the borrow record
        //        var libraryBook = await _connect.LibraryBooks
        //            .FirstOrDefaultAsync(lb => lb.BookId == borrowRecord.BookId && lb.LibraryId == borrowRecord.LibraryId);

        //        if (libraryBook != null)
        //        {
        //            // Increase the stock in the library
        //            libraryBook.Stock++;
        //        }
        //    }

        //    await _connect.SaveChangesAsync();
        //    return true;
        //}

        public async Task<bool> UpdateBorrowRecordAsync(Borrow borrow)
        {
            _connect.Borrows.Update(borrow);
            return await _connect.SaveChangesAsync() > 0;
        }
        public async Task<Borrow> GetBorrowRecordByIdAsync(int borrowId)
        {
            return await _connect.Borrows
                .Include(b => b.Book)    // Include book details if needed
                .Include(b => b.Member)  // Include member details if needed
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);  // Fetch borrow record by borrowId
        }


        public async Task<bool> UpdateLibraryStockOnReturnAsync(Borrow borrowRecord)
        {
            if (borrowRecord == null)
                return false;

            // ✅ LibraryId fetch karo from Memberships table
            var libraryId = await _connect.Memberships
                .Where(ms => ms.MemberId == borrowRecord.MemberId)
                .Select(ms => ms.LibraryId)
                .FirstOrDefaultAsync();

            if (libraryId == 0)
            {
                Console.WriteLine($"LibraryId not found for MemberId: {borrowRecord.MemberId}");
                return false;
            }

            // ✅ Find the LibraryBooks record
            var libraryBook = await _connect.LibraryBooks
                .FirstOrDefaultAsync(lb => lb.BookId == borrowRecord.BookId && lb.LibraryId == libraryId);

            if (libraryBook == null)
            {
                Console.WriteLine($"LibraryBook entry not found for BookId: {borrowRecord.BookId}, LibraryId: {libraryId}");
                return false;
            }

            try
            {
                libraryBook.Stock++; // ✅ Increase the stock properly
                await _connect.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stock: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Borrow>> GetBorrowedBooksByMemberIdAsync(int memberId)
        {
            return await _connect.Borrows
        .Where(b => b.MemberId == memberId)  // MemberId ke basis pe filter kar raha hai
        .Include(b => b.Book)   // Book details include karega
        .ToListAsync();
        }

        //public async Task<bool> UpdateLibraryStockOnReturnAsync(Borrow borrowRecord)
        //{
        //    if (borrowRecord == null)
        //        return false;

        //    // ✅ Get LibraryId from Membership table
        //    var libraryId = await _connect.Members
        //       .Where(m => m.Id == borrowRecord.MemberId)
        //       .Select(m => m.Memberships.Select(ms => ms.LibraryId).FirstOrDefault())  // ✅ Membership table ka use karo
        //       .FirstOrDefaultAsync();


        //    if (libraryId == 0)
        //        return false;

        //    // ✅ Find the LibraryBooks record
        //    var libraryBook = await _connect.LibraryBooks
        //        .FirstOrDefaultAsync(lb => lb.BookId == borrowRecord.BookId && lb.LibraryId == libraryId);

        //    if (libraryBook != null)
        //    {
        //        libraryBook.Stock++; // ✅ Increase the stock properly
        //        await _connect.SaveChangesAsync();
        //        return true;
        //    }

        //    return false;
        //}


    }
}
