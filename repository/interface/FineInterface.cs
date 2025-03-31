using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface FineInterface
    {
        Task<Fine> GetFineByIdAsync(int fineId);
        Task<Fine> GetFineByBorrowIdAsync(int borrowId);
        Task<bool> AddFineAsync(Fine fine);
        Task<bool> UpdateFineAsync(Fine fine);

        Task<decimal> CalculateFineAsync(int borrowId);

        byte[] GenerateFineReceiptPdf(Fine fine);

        // Task<bool> CalculateFineAsync(int borrowId);
        // decimal CalculateFine(int borrowId);
    }
}
