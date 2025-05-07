using library_management.Models;
using library_management.repository.classes;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using static library_management.Models.Book;
using System.IO.Compression;
using OfficeOpenXml;
using ClosedXML.Excel;
using System.Diagnostics;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace library_management.Controllers
{
    public class BookMasterController : BaseController
    {
        private readonly dbConnect _context;
        private readonly BookServiceInterface _bookServiceInterface;
        private readonly PermisionHelperInterface _permission;
        private readonly IActivityRepository _activityRepository;
        private readonly libraryInterface _libraryInterface;
        public BookMasterController(dbConnect connect, BookServiceInterface bookServiceInterface, ISidebarRepository sidebar, PermisionHelperInterface permission, IActivityRepository activityRepository, libraryInterface libraryInterface) : base(sidebar)
        {
            _context = connect;
            _bookServiceInterface = bookServiceInterface;
            _permission = permission;
            _activityRepository = activityRepository;
            _libraryInterface = libraryInterface;
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

        //public async Task<IActionResult> BookList()
        //{
        //    string permissionType = GetUserPermission("View Book");
        //    if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
        //    {
        //        //var (books, bookLibraryMap) = await _bookServiceInterface.GetAllbooksData();
        //        //ViewBag.BookLibraryMap = bookLibraryMap;
        //        //return View(books);
        //        var books = await _bookServiceInterface.GetAllbooksData(); //  No Extra Parameters
        //        return View(books);
        //    }
        //    else
        //    {
        //        return RedirectToAction("UnauthorisedAccess", "Error");
        //    }

        //}
        public async Task<IActionResult> BookList(int? libraryId)
        {
            string permissionType = GetUserPermission("View Book");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                int roleId = _bookServiceInterface.GetLoggedInUserRoleId();
                ViewBag.RoleId = roleId;

                // Libraries for dropdown (SuperAdmin only)
                if (roleId == 1)
                {
                    var librariesWithBooks = await _context.LibraryBooks
                        .Include(lb => lb.Library)
                        .Where(lb => lb.BookId != null)
                        .Select(lb => lb.Library)
                        .Distinct()
                        .ToListAsync();

                    ViewBag.Libraries = librariesWithBooks
                        .Select(lib => new SelectListItem
                        {
                            Value = lib.LibraryId.ToString(),
                            Text = lib.Libraryname
                        }).ToList();
                }

                // Get filtered book list by library
                var books = await _bookServiceInterface.GetAllbooksData(libraryId);
                ViewBag.SelectedLibraryId = libraryId; // Optional: To keep dropdown selected

                return View(books);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


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
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            int libraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId(); //  Service Se Call
            if (ModelState.IsValid)
            {
                var result = await _bookServiceInterface.AddBook(book, libraryId, stock);

                //  Ensure result is correctly interpreted
                bool success = (bool)result.GetType().GetProperty("success").GetValue(result);
                string message = (string)result.GetType().GetProperty("message").GetValue(result);

                if (success)
                {
                    string type = "added book";
                    string desc = $"{userName} added new book in his library";
                    _activityRepository.AddNewActivity(id, type, desc);
                    return Json(new { success = true, message = message });
                }

                return Json(new { success = false, message = message });

                //if (success)
                //{
                //    TempData["SuccessMessage"] = message;
                //    return RedirectToAction("BookList");
                //}

                //ModelState.AddModelError("", message);
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



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult DeleteBookFromLibrary(int id)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int? libraryId = GetLoggedInLibraryId();


            if (libraryId == null)
            {
                return Json(new { success = false, message = "Library not found for this admin." });
            }

            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid book id." });
            }

            try
            {
                // Remove stock of the book from this admin's library
                var libraryBook = _context.LibraryBooks
                                          .FirstOrDefault(x => x.BookId == id && x.LibraryId == libraryId);

                if (libraryBook == null)
                {
                    return Json(new { success = false, message = "Book not found in this library." });
                }

                _context.LibraryBooks.Remove(libraryBook);
                _context.SaveChanges();

                string bookTitle = _context.Books.Where(b => b.BookId == id).Select(b => b.Title).FirstOrDefault();
                string desc = $"Book '{bookTitle}' was removed from library (ID: {libraryId}) by user ID {userId}.";

                _activityRepository.AddNewActivity(userId, "delete book", desc);

                return Json(new { success = true, message = "Book removed from your library successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing book from library: " + ex.Message);
                return Json(new { success = false, message = "Error removing book from library." });
            }
        }




        //[HttpPost]
        ////[ValidateAntiForgeryToken] // Protect against CSRF attacks
        //public IActionResult DeleteUser(int id)
        //{
        //    int userid = (int)HttpContext.Session.GetInt32("UserId");
        //    string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
        //    if (id == 0)
        //    {
        //        return Json(new { success = false, message = "Invalid user id." });
        //    }

        //    try
        //    {
        //        var book = _context.Books.Find(id);
        //        if (book == null)
        //        {
        //            return Json(new { success = false, message = "User not found." });
        //        }

        //        _context.Books.Remove(book);
        //        _context.SaveChanges();

        //        string type = "delete book";
        //        string desc = $"{userName} deleted book in his library";
        //        _activityRepository.AddNewActivity(id, type, desc);

        //        return Json(new { success = true, message = "user deleted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error deleting user: " + ex.Message);
        //        return Json(new { success = false, message = "Error deleting user." });
        //    }
        //}



        public IActionResult DownloadSampleFile()
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            // Create a new workbook & sheet
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sample");

            // Add headers

            sheet.Cell(1, 1).Value = "Book Title";
            sheet.Cell(1, 2).Value = "GenreId";  // Dropdown applied here
            sheet.Cell(1, 3).Value = "ISBN";
            sheet.Cell(1, 4).Value = "Publisher";
            sheet.Cell(1, 5).Value = "Publication Year";
            sheet.Cell(1, 6).Value = "Edition";
            sheet.Cell(1, 7).Value = "Language";
            sheet.Cell(1, 8).Value = "Author";
            sheet.Cell(1, 9).Value = "Book Image Path";
            sheet.Cell(1, 10).Value = "Stock";

            // Fetch categories from DB
            var categories = _context.Genres.Select(c => c.GenreName).ToList();

            // Add categories to a hidden sheet
            var categorySheet = workbook.Worksheets.Add("Genres");

            for (int i = 0; i < categories.Count; i++)
            {
                categorySheet.Cell(i + 1, 1).Value = categories[i];
            }

            // Hide the category sheet (so users don't see it)
            categorySheet.Hide();

            // Apply dropdown list to Category column (B2:B100)
            var categoryRange = sheet.Range("B2:B100");
            var categoryValidation = categoryRange.CreateDataValidation();
            categoryValidation.List(categorySheet.Range($"A1:A{categories.Count}"));

            // Save to MemoryStream & return as file
            byte[] fileBytes;

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                fileBytes = stream.ToArray();
            }
            string type = "added book";
            string desc = $"{userName} added new book in his library";
            _activityRepository.AddNewActivity(id, type, desc);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleFile.xlsx");

        }



        [HttpPost]
        public async Task<IActionResult> UploadSampleFile(IFormFile excelFile, IFormFile imageZip)
        {

            int id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();

            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var stream = new MemoryStream();

            if (excelFile == null || imageZip == null)
                return BadRequest("Please upload both Excel file and Image ZIP folder.");

            if (!imageZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .zip files are allowed for image upload." });
            }
            // Extract ZIP file
            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Bookimages");
            Directory.CreateDirectory(imageFolderPath);

            using (var zipStream = new MemoryStream())
            {
                await imageZip.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string extension = Path.GetExtension(entry.Name).ToLower();

                        // ❌ Reject non-image files
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, message = "only image files are allowed inside the zip" });
                        }

                        // ✅ Move valid images
                        var filePath = Path.Combine(imageFolderPath, entry.Name);
                        if (!System.IO.File.Exists(filePath))
                        {
                            entry.ExtractToFile(filePath);
                        }
                    }
                }
            }
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "File is empty." });
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .xlsx files are allowed." });
            }

            //Class file code
            try
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");



                var resultList = new List<ImportStatusModel>();

                excelFile.CopyTo(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheet(1);
                    var rows = sheet.RowsUsed().Skip(1); // Skip header row
                    int libraryId = _bookServiceInterface.GetLoggedInLibrarianLibraryId(); //  Service Se Call
                    foreach (var row in rows)
                    {
                        var BookTitle = row.Cell(1).GetString();
                        var GenreName = row.Cell(2).GetString();
                        var ISBN = row.Cell(3).GetString();
                        var Publisher = row.Cell(4).GetString();
                        var PublicationYear = row.Cell(5).GetValue<int>();
                        var Edition = row.Cell(6).GetString();
                        var Language = row.Cell(7).GetString();
                        var Author = row.Cell(8).GetString();
                        var ImagePath = row.Cell(9).GetString();
                        var Stock = row.Cell(10).GetValue<int>();

                        var importStatus = new ImportStatusModel
                        {
                            Name = BookTitle,
                            Status = "Pending",
                            Message = ""
                        };

                        // 🛠 Validate Data
                        if (string.IsNullOrEmpty(BookTitle))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "BookTitle are required.";
                        }
                        else if (string.IsNullOrEmpty(GenreName) || GenreName == "0")
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "GenreName is required.";
                        }
                        else if (string.IsNullOrEmpty(ISBN))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "ISBN is required.";
                        }
                        else if (PublicationYear == 0)
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "PublicationYear is required.";
                        }
                        else if (string.IsNullOrEmpty(Publisher))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "Publisher is required.";
                        }
                        else if (string.IsNullOrEmpty(Edition))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "Edition is required.";
                        }
                        else if (string.IsNullOrEmpty(Language))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "Language is required.";
                        }
                        else if (string.IsNullOrEmpty(Author))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "Author is required.";
                        }
                        else if (string.IsNullOrEmpty(ImagePath))
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "ImagePath is required.";
                        }
                        else if (Stock == 0)
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = "Stock is required.";
                        }

                        else
                        {
                            // 🛠 Fetch required data BEFORE making async calls
                            var genre = await _context.Genres
                                .Where(x => x.GenreName == GenreName)
                                .Select(x => x.GenreId)
                                .FirstOrDefaultAsync();

                            if (genre == 0)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Invalid genre";
                            }
                            else
                            {
                                // ✅ Insert into tblUsers
                                var book = new Book
                                {
                                    Title = BookTitle,
                                    Publisher = Publisher,
                                    PublicationYear = PublicationYear,
                                    GenreId = genre,
                                    Isbn = ISBN,
                                    Edition = Edition,
                                    Language = Language,
                                    Author = Author,
                                    bookimagepath = @"\Bookimages\" + ImagePath
                                };

                                await _bookServiceInterface.AddBook(book, libraryId, Stock); // Ensure SaveUsers is properly async

                                importStatus.Status = "Success";
                            }
                        }

                        resultList.Add(importStatus);
                    }
                }
                //Output File Generation

                using (var workbook = new XLWorkbook())
                {
                    var sheet = workbook.Worksheets.Add("Import Status");

                    // Headers
                    sheet.Cell("A1").Value = "Name";
                    sheet.Cell("B1").Value = "Status";
                    sheet.Cell("C1").Value = "Message";

                    int row = 2;
                    foreach (var result in resultList)
                    {
                        sheet.Cell(row, 1).Value = result.Name;
                        sheet.Cell(row, 2).Value = result.Status;
                        sheet.Cell(row, 3).Value = result.Message;
                        row++;
                    }

                    workbook.SaveAs(stream);
                }

                var fileBytes = stream.ToArray();

                // Convert to Base64 to store temporarily
                string fileBase64 = Convert.ToBase64String(fileBytes);
                string type = "added book";
                string desc = $"{userName} added new book in his library";
                _activityRepository.AddNewActivity(id, type, desc);
                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully!",
                    fileData = fileBase64,
                    fileName = "OutputOfSampleFile.xlsx"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
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


        public async Task<IActionResult> GetBooksByLibrary(int libraryId)
        {
            // Fetch books along with their related genre and library using Include()
            var books = await _context.LibraryBooks
                .Where(lb => lb.LibraryId == libraryId)
                .Include(lb => lb.Book)        // Include the Book details
                .Include(lb => lb.Book.Genre)  // Include the Genre details for each book
                .Include(lb => lb.Library)     // Include the Library details for each book
                .ToListAsync();

            var html = new StringBuilder();

            foreach (var libraryBook in books)
            {
                var book = libraryBook.Book;  // Get the Book entity
                var genreName = book.Genre?.GenreName ?? "-"; // Fetch Genre Name (Handle null)
                var libraryName = libraryBook.Library?.Libraryname ?? "-"; // Fetch Library Name (Handle null)

                // Build the table row for each book
                html.AppendLine("<tr>");
                html.AppendLine($"<td><img src='{book.bookimagepath}' class='img-fluid rounded' style='width: 50px; height: 70px;' /></td>");
                html.AppendLine($"<td>{book.Title}</td>");
                html.AppendLine($"<td>{book.Author ?? "-"}</td>");
                html.AppendLine($"<td>{genreName}</td>");
                html.AppendLine($"<td>{libraryName}</td>");
                html.AppendLine("<td>");
                html.AppendLine($"<a class='btn btn-info' href='/BookMaster/Details/{book.BookId}'><i class='fas fa-eye'></i></a>");
                html.AppendLine($"<a class='btn btn-primary' href='/BookMaster/Edit/{book.BookId}'><i class='fas fa-edit'></i></a>");
                html.AppendLine($"<button class='btn btn-danger delete-btn' data-id='{book.BookId}'><i class='fas fa-trash-alt'></i></button>");
                html.AppendLine("</td>");
                html.AppendLine("</tr>");
            }

            return Content(html.ToString(), "text/html");
        }




    }
}
