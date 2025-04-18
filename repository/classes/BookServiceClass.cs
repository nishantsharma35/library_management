using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;

namespace library_management.repository.classes
{
    public class BookServiceClass : BookServiceInterface
    {
        private readonly dbConnect _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public BookServiceClass(dbConnect connect, IHttpContextAccessor httpContextAccessor)
        {
            _context = connect;
            _httpContextAccessor = httpContextAccessor;
        }

        //public bool IsSuperAdmin()
        //{
        //    return _httpContextAccessor.HttpContext.Session.GetString("UserRole") == "SuperAdmin";
        //}




        public int GetLoggedInLibrarianLibraryId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("LibraryId") ?? 0;

        }

        public int GetLoggedInUserRoleId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserRoleId") ?? 0;
        }


        public async Task<List<Book>> GetAllbooksData()
        {
            int roleId = GetLoggedInUserRoleId();

            if (roleId == 1)
            {
                return await _context.Books
                    .Include(b => b.Genre)
                    .ToListAsync();
            }
            else
            {
                int libraryId = GetLoggedInLibrarianLibraryId();

                return await _context.LibraryBooks
                    .Where(lb => lb.LibraryId == libraryId)
                    .Include(lb => lb.Book)
                        .ThenInclude(b => b.Genre)
                    .Select(lb => lb.Book)
                    .ToListAsync();
            }
        }


        public async Task<Book> GetBookById(int id)
        {
            return await _context.Books
                .Include(b => b.Genre) // Include Genre data
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task<object> AddBook(Book book, int libraryId, int stock)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Isbn == book.Isbn);
            string imgPath  = "";

            if(book.bookimagepath != null)
            {
                imgPath = book.bookimagepath;
            }

            // Image handling logic
            if (book.bookImage != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Bookimages");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + book.bookImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    book.bookImage.CopyTo(stream);
                }

                imgPath = "\\Bookimages\\" + uniqueFileName;
            }

            string msg;
            if (existingBook == null)
            {
                // ✅ **New Book Add**
                book.bookimagepath = imgPath;
                _context.Books.Add(book);
                await _context.SaveChangesAsync(); // Save to get BookId

                if (book.BookId <= 0)
                {
                    return new { success = false, message = "Failed to add book, please try again." };
                }

                // **Add entry in LibraryBooks**
                _context.LibraryBooks.Add(new LibraryBook { BookId = book.BookId, LibraryId = libraryId, Stock = stock });
                msg = "New book added successfully.";
            }
            else
            {
                // ✅ **Update Existing Book**
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Genre = book.Genre;
                existingBook.Edition = book.Edition;   // ✅ **Update Edition**
                existingBook.Language = book.Language; // ✅ **Update Language**
                existingBook.bookimagepath = !string.IsNullOrEmpty(imgPath) ? imgPath : existingBook.bookimagepath;

                var libraryBook = await _context.LibraryBooks
                    .FirstOrDefaultAsync(lb => lb.BookId == existingBook.BookId && lb.LibraryId == libraryId);

                if (libraryBook == null)
                {
                    _context.LibraryBooks.Add(new LibraryBook { BookId = existingBook.BookId, LibraryId = libraryId, Stock = stock });
                }
                else
                {
                    libraryBook.Stock = stock; // ✅ Directly set new stock (no unnecessary increments)
                }

                msg = "Book updated successfully.";
            }

            await _context.SaveChangesAsync();
            return new { success = true, message = msg };
        }







        //public async Task<object> AddBook(Book book, int libraryId, int stock)
        //{
        //    var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Isbn == book.Isbn);
        //    string imgPath = "";

        //    if (book.bookImage != null)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Bookimages");
        //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + book.bookImage.FileName;
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            book.bookImage.CopyTo(stream);
        //        }

        //        imgPath = "\\Bookimages\\" + uniqueFileName;
        //    }

        //    string msg;
        //    if (existingBook == null)
        //    {
        //        // Naya book add karna hai
        //        book.bookimagepath = imgPath;
        //        _context.Books.Add(book);
        //        await _context.SaveChangesAsync(); // Book ID milne ke liye save karna padega

        //        // **Debugging Log**
        //        Console.WriteLine($"Adding New Book: {book.Title}, ID: {book.BookId}");

        //        if (book.BookId <= 0)
        //        {
        //            return new { success = false, message = "Failed to add book, please try again." };
        //        }

        //        // LibraryBooks table me insert karna hoga
        //        _context.LibraryBooks.Add(new LibraryBook { BookId = book.BookId, LibraryId = libraryId, Stock = stock });
        //        msg = "New book added successfully.";
        //    }
        //    else
        //    {
        //        // Agar book pehle se hai, uska stock update hoga
        //        existingBook.bookimagepath = !string.IsNullOrEmpty(imgPath) ? imgPath : existingBook.bookimagepath;

        //        var libraryBook = await _context.LibraryBooks
        //            .FirstOrDefaultAsync(lb => lb.BookId == existingBook.BookId && lb.LibraryId == libraryId);

        //        if (libraryBook == null)
        //        {
        //            _context.LibraryBooks.Add(new LibraryBook { BookId = existingBook.BookId, LibraryId = libraryId, Stock = stock });
        //        }
        //        else
        //        {
        //            libraryBook.Stock += stock;
        //            _context.LibraryBooks.Update(libraryBook);
        //        }

        //        // **Debugging Log**
        //        Console.WriteLine($"Updating Stock for Existing Book: {existingBook.Title}, ID: {existingBook.BookId}");

        //        msg = "Book stock updated.";
        //    }

        //    await _context.SaveChangesAsync();
        //    return new { success = true, message = msg };
        //}


        //public async Task<object> AddBook(Book book, int libraryId, int stock)
        //{
        //    string imgPath = "";

        //    // 📌 Step 1: Book Image Upload Handling
        //    if (book.bookImage != null)
        //    {
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        //        string fileExtension = Path.GetExtension(book.bookImage.FileName).ToLower();

        //        if (!allowedExtensions.Contains(fileExtension))
        //        {
        //            return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
        //        }

        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Bookimages");
        //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + book.bookImage.FileName;
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await book.bookImage.CopyToAsync(stream);
        //        }

        //        imgPath = "\\Bookimages\\" + uniqueFileName;
        //    }

        //    string msg = "";
        //    var existingBook = await _context.Books.FirstOrDefaultAsync(u => u.BookId == book.BookId);

        //    // 📌 Step 2: Book Insert / Update Logic
        //    if (existingBook != null) // Agar Book pehle se exist karti hai to update karo
        //    {
        //        existingBook.Title = book.Title;
        //        existingBook.Author = book.Author;
        //        existingBook.Isbn = book.Isbn;
        //        existingBook.Publisher = book.Publisher;
        //        existingBook.PublicationYear = book.PublicationYear;
        //        existingBook.Genre = book.Genre;

        //        if (book.bookImage != null) // Agar naye image upload hui hai to update karo
        //        {
        //            existingBook.bookimagepath = imgPath;
        //        }

        //        msg = "Book data has been updated successfully.";
        //    }
        //    else // Nayi book add ho rahi hai
        //    {
        //        await _context.Books.AddAsync(book);
        //        msg = "New book has been added successfully.";
        //    }

        //    await _context.SaveChangesAsync(); // 📌 Step 3: Save Book Table Changes

        //    // **⚠️ Ensure BookId is properly set for new books**
        //    int bookId = existingBook != null ? existingBook.BookId : book.BookId;

        //    // 📌 Step 4: LibraryBooks Table Insert / Update Logic
        //    var existingLibraryBook = await _context.LibraryBooks
        //        .FirstOrDefaultAsync(lb => lb.BookId == bookId && lb.LibraryId == libraryId);

        //    if (existingLibraryBook != null) // Agar book pehle se iss library me hai to stock update karo
        //    {
        //        existingLibraryBook.Stock += stock;
        //        _context.LibraryBooks.Update(existingLibraryBook);
        //    }
        //    else // Nayi entry banani hai
        //    {
        //        var newLibraryBook = new LibraryBook
        //        {
        //            BookId = bookId,
        //            LibraryId = libraryId,
        //            Stock = stock
        //        };
        //        await _context.LibraryBooks.AddAsync(newLibraryBook);
        //    }

        //    await _context.SaveChangesAsync(); // 📌 Step 5: Save LibraryBooks Table Changes

        //    return new { success = true, message = msg };
        //}


        public async Task<Book> GetBookDetailsById(int bookId, int libraryId)
        {
            return await _context.LibraryBooks
                .Where(lb => lb.BookId == bookId && lb.LibraryId == libraryId)
                .Include(lb => lb.Book)
                    .ThenInclude(b => b.Genre)
                .Select(lb => new Book
                {
                    BookId = lb.Book.BookId,
                    Title = lb.Book.Title,
                    Author = lb.Book.Author,
                    Genre = lb.Book.Genre,
                    Stock = lb.Stock
                })
                .FirstOrDefaultAsync();
        }




        public async Task<List<Book>> GetAvailableBooksByLibraryAsync(int libraryId)
        {
            var libraryBookStocks = await _context.LibraryBooks
        .Where(lb => lb.LibraryId == libraryId)
        .Select(lb => new { lb.BookId, lb.Stock })
        .ToListAsync();

            // ✅ Step 2: Fetch all borrowed book IDs and count how many copies are borrowed
            var borrowedBookCounts = await _context.Borrows
                .Where(b => b.LibraryId == libraryId && b.ReturnDate == null)
                .GroupBy(b => b.BookId)
                .Select(g => new { BookId = g.Key, BorrowedCount = g.Count() })
                .ToListAsync();

            // ✅ Step 3: Get books that have remaining stock (Stock - BorrowedCount > 0)
            var availableBookIds = libraryBookStocks
                .Where(lb => lb.Stock - (borrowedBookCounts.FirstOrDefault(b => b.BookId == lb.BookId)?.BorrowedCount ?? 0) > 0)
                .Select(lb => lb.BookId)
                .ToList();

            // ✅ Step 4: Fetch book details for available books
            var availableBooks = await _context.Books
                .Where(b => availableBookIds.Contains(b.BookId))
                .ToListAsync();

            return availableBooks;


            //      var libraryBookIds = await _context.LibraryBooks
            //  .Where(lb => lb.LibraryId == libraryId)
            //  .Select(lb => lb.BookId)
            //  .Distinct()
            //  .ToListAsync();

            //      // ✅ Ab check karo ki kaunsi books currently borrow ho chuki hain
            //      var borrowedBookIds = await _context.Borrows
            //          .Where(b => b.LibraryId == libraryId && b.ReturnDate == null)
            //          .Select(b => b.BookId)
            //          .ToListAsync();

            //      // ✅ Jo books borrow nahi hui hain, wahi availableBooks me dikhani hain
            //      var availableBooks = await _context.LibraryBooks
            //.Where(lb => lb.LibraryId == libraryId) // ✅ Sirf specific library ki books lo
            //.Select(lb => lb.Book) // ✅ Book ka data fetch karo
            //.ToListAsync();


            //      return availableBooks;
        }


        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            var borrowedBookIds = await _context.Borrows
                    .Where(b => b.ReturnDate == null) // Jo abhi tak return nahi hui
                    .Select(b => b.BookId)
                    .ToListAsync();

            var availableBooks = await _context.Books
                .Where(b => !borrowedBookIds.Contains(b.BookId)) // Borrowed me nahi honi chahiye
                .ToListAsync();

            return availableBooks;
        }

        public async Task<bool> IsBookAvailableInLibraryAsync(int bookId, int libraryId)
        {
            return await _context.LibraryBooks.AnyAsync(lb => lb.BookId == bookId && lb.LibraryId == libraryId);
        }


    }
}
