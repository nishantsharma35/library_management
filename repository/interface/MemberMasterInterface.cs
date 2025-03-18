using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface MemberMasterInterface
    {
        Task<List<Member>> GetAllMembersAsync();

        Task<List<Member>> GetLibraryMembersAsync(int libraryId);
        Task<object> UpdateStatus(int adminId, bool status);

        Task<object> AddMember(Member user);
        Member checkExistence(string Username, string email, int UserId);

    }
}
