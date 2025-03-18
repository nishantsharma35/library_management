using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface MembershipInterface
    {
        Task<List<Membership>> GetAllMemberships(bool includePending = false);
        Task<Membership?> GetMembershipById(int id);
        Task<bool> AddMembership(Membership membership);
        Task<List<Membership>> GetMembershipsByMemberIdAsync(int memberId);
        //Task<bool> UpdateMembership(Membership membership);
        //Task<bool> DeleteMembership(int id);

    }
}
