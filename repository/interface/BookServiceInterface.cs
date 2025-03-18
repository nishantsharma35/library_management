using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface BookServiceInterface
    {
        Task<List<Book>> GetAllbooksData();

        Task<List<Book>> GetAvailableBooksAsync();
       Task<List<Book>> GetAvailableBooksByLibraryAsync(int libraryId);

        Task<Book> GetBookById(int id);

        int GetLoggedInLibrarianLibraryId();
        int GetLoggedInUserRoleId();

        Task<Book> GetBookDetailsById(int bookId, int libraryId);
        Task<object> AddBook(Book book, int libraryId, int stock);

        Task<bool> IsBookAvailableInLibraryAsync(int bookId, int libraryId);
    }
}
