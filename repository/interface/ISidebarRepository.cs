using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface ISidebarRepository
    {
        Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId);
        public Member GetMember(int id);
    }   
}
