using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface BorrowInterface
    {
        Task<bool> BorrowBookAsync(int memberId, int bookId, int libraryId, DateTime issueDate, DateTime dueDate);

        Task<bool> UpdateLibraryStockOnReturnAsync(Borrow borrowRecord);
        Task<bool> ReturnBookAsync(int borrowId);
        Task<List<Borrow>> GetAllBorrowsAsync(int libraryId);
        Task<bool> UpdateBorrowRecordAsync(Borrow borrow);

        Task<Borrow> GetBorrowRecordByIdAsync(int borrowId);

        Task<List<Borrow>> GetBorrowedBooksByMemberIdAsync(int memberId);
    }
}
