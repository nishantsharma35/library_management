using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface AdminInterface
    {
        Task<List<Library>> GetAllAdminsData();

        Task<object> UpdateStatus(int mainadminId, bool status);
        Task<object> AddAdmin(Library admin);

        Library checkExistence(string name, int UserId);

    }
}
