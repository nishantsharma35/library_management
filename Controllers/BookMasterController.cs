using library_management.Models;
using library_management.repository.classes;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using static library_management.Models.Book;

namespace library_management.Controllers
{
    public class BookMasterController : BaseController
    {
        private readonly dbConnect _context;
        private readonly BookServiceInterface _bookServiceInterface;
        private readonly PermisionHelperInterface _permission;
        public BookMasterController(dbConnect connect, BookServiceInterface bookServiceInterface, ISidebarRepository sidebar, PermisionHelperInterface permission) : base(sidebar)
        {
            _context = connect;
            _bookServiceInterface = bookServiceInterface;
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

        public async Task<IActionResult> BookList()
        {
            string permissionType = GetUserPermission("View Book");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                var books = await _bookServiceInterface.GetAllbooksData(); //  No Extra Parameters
                return View(books);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
           
        }


        //public async Task<IActionResult> booklist()
        //{
        //    var books = await _bookServiceInterface.GetAllbooksData();
        //    foreach (var book in books)
        //    {
        //        Console.WriteLine($"Book: {book.Title}, Genre: {(book.Genre != null ? book.Genre.GenreName : "NULL")}");
        //    }
        //    return View(books);

        //}


        [HttpGet]
        public IActionResult Details(int id)
        {
            string permissionType = GetUserPermission("View Book");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                int libraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId();
                int userRoleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;

                //  Pehle LibraryBooks table se check karo ki book exist karti hai ya nahi
                var libraryBookQuery = _context.LibraryBooks
                    .Include(lb => lb.Book)
                    .ThenInclude(b => b.Genre)
                    .Where(lb => lb.BookId == id);

                //  Agar user Librarian hai, to sirf uski library ki books dikhani hai
                if (userRoleId != 1)
                {
                    libraryBookQuery = libraryBookQuery.Where(lb => lb.LibraryId == libraryId);
                }

                var libraryBook = libraryBookQuery.FirstOrDefault();

                if (libraryBook == null)
                {
                    return NotFound(); // ✅ Agar book exist nahi karti ya access nahi allowed, to 404
                }

                // ✅ Final Book object create karna
                var book = libraryBook.Book;
                book.Stock = libraryBook.Stock; // ✅ Stock bhi assign kar diya

                return View(book);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }


        //[HttpGet]
        //public IActionResult Details(int id)
        //{
        //    var book = _context.Books
        //        .Include(b => b.Genre) // Genre table ka data include karna zaroori hai
        //        .FirstOrDefault(m => m.BookId == id);

        //    if (book == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(book);
        //}

        [HttpGet]
        public IActionResult AddBook(int? id)
        {
            string permissionType = GetUserPermission("Add Book");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                ViewBag.Genres = _context.Genres.ToList();
                ViewBag.LibraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId();
                // Genre dropdown ke liye data

                if (id == null || id == 0)
                {
                    return View(new Book()); // Add Book case
                }
                else
                {
                    var book = _context.Books.FirstOrDefault(b => b.BookId == id);
                    if (book == null)
                    {
                        return NotFound();
                    }
                    return View(book); // Edit case
                }
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }


        [HttpPost]
        public async Task<IActionResult> AddBook(Book book, int stock)
        {
            int libraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId(); //  Service Se Call
            if (ModelState.IsValid)
            {
                var result = await _bookServiceInterface.AddBook(book, libraryId, stock);

                //  Ensure result is correctly interpreted
                bool success = (bool)result.GetType().GetProperty("success").GetValue(result);
                string message = (string)result.GetType().GetProperty("message").GetValue(result);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("BookList");
                }

                ModelState.AddModelError("", message);
            }
            ViewBag.Genres = _context.Genres.Any() ? _context.Genres.ToList() : new List<Genre>();
            ViewBag.LibraryId = libraryId; // Error aaye to LibraryId retain ho
            return View(book);
        }


        //public IActionResult AddBook()
        //{
        //    int librarianLibraryId = GetLoggedInLibrarianLibraryId(); // Get librarian's LibraryId
        //    ViewBag.LibrarianLibraryId = librarianLibraryId; // Pass to View

        //    // Create an instance of BookViewClass
        //    var bookView = new BookViewClass();

        //    return View(bookView); // Pass correct model
        //}



        //        [HttpPost]
        //public async Task<IActionResult> AddBook(Book book, int stock)
        //{
        //    try
        //    {
        //        int librarianLibraryId = GetLoggedInLibrarianLibraryId();
        //        var result = await _bookServiceInterface.AddBook(book, librarianLibraryId, stock);

        //        if ((bool)result.GetType().GetProperty("success")?.GetValue(result, null))
        //        {
        //            TempData["SuccessMessage"] = result.GetType().GetProperty("message")?.GetValue(result, null)?.ToString();
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", result.GetType().GetProperty("message")?.GetValue(result, null)?.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", "An error occurred while adding the book: " + ex.Message);
        //    }

        //    return View(book);
        //}
        [HttpPost]
        //[ValidateAntiForgeryToken] // Protect against CSRF attacks
        public IActionResult DeleteUser(int id)
        {
            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid user id." });
            }

            try
            {
                var book = _context.Books.Find(id);
                if (book == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                _context.Books.Remove(book);
                _context.SaveChanges();

                return Json(new { success = true, message = "user deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return Json(new { success = false, message = "Error deleting user." });
            }
        }

        [HttpGet]
        public IActionResult BulkUpload()
        {
            string permissionType = GetUserPermission("Bulk book adding");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                return View(); // Ensure that BulkUpload.cshtml exists in Views/BookMaster
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }

           
        }


        [HttpPost]
        public IActionResult BulkUpload(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["Error"] = "Please upload a valid CSV file.";
                return RedirectToAction("BulkUpload");
            }

            try
            {
                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<Book.BookCsvMap>(); // ✅ CSV Mapping
                    var books = csv.GetRecords<Book>().ToList();

                    if (books.Any())
                    {
                        int newBooksAdded = 0;
                        int existingBooksUpdated = 0;

                        // ✅ Step 2: Session me LibraryId set/update karo
                        int? sessionLibraryId = HttpContext.Session.GetInt32("LibraryId");

                        if (sessionLibraryId == null || sessionLibraryId == 0)
                        {
                            int newLibraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId();
                            HttpContext.Session.SetInt32("LibraryId", newLibraryId);
                            sessionLibraryId = newLibraryId;
                        }

                        int libraryId = sessionLibraryId.Value;
                        Console.WriteLine($" Using Library ID: {libraryId}");

                        foreach (var book in books)
                        {
                            var existingBook = _context.Books.FirstOrDefault(b => b.Isbn == book.Isbn);

                            if (existingBook != null)
                            {
                                var libraryBook = _context.LibraryBooks
                                    .FirstOrDefault(lb => lb.BookId == existingBook.BookId && lb.LibraryId == libraryId);

                                if (libraryBook != null)
                                {
                                    libraryBook.Stock += 1; // ✅ Stock update ho raha hai
                                    existingBooksUpdated++;
                                }
                                else
                                {
                                    _context.LibraryBooks.Add(new LibraryBook
                                    {
                                        BookId = existingBook.BookId,
                                        LibraryId = libraryId,
                                        Stock = 1
                                    });
                                    newBooksAdded++;
                                }
                            }
                            else
                            {
                                _context.Books.Add(book);
                                _context.SaveChanges(); // ✅ ID generate karne ke liye SaveChanges() zaroori hai

                                _context.LibraryBooks.Add(new LibraryBook
                                {
                                    BookId = book.BookId,
                                    LibraryId = libraryId,
                                    Stock = 1
                                });

                                newBooksAdded++;
                            }
                        }
                        _context.SaveChanges(); // ✅ Sab changes save karne ke liye

                        Console.WriteLine($" Import Completed: {newBooksAdded} new books added, {existingBooksUpdated} books updated in stock.");
                        TempData["Success"] = $" Import Completed: {newBooksAdded} new books added, {existingBooksUpdated} books updated in stock.";
                    }
                    else
                    {
                        Console.WriteLine(" No valid books found in the CSV file.");
                        TempData["Error"] = " No valid books found in the CSV file.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing file: {ex.Message}";
            }

            return RedirectToAction("BulkUpload");
        }


    }
}
