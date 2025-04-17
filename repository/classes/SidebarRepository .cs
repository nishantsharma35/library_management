using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;

namespace library_management.repository.classes
{
    public class SidebarRepository : ISidebarRepository
    {
        private readonly dbConnect _context;

        public SidebarRepository(dbConnect context)
        {
            _context = context;
        }

        public Member GetMember(int id)
        {
            return _context.Members.Where(x => x.Id == id).Select(member => new Member
            {
                Id = member.Id,
                Name = member.Name,
                Picture = member.Picture
            }).FirstOrDefault();
        }

        public async Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId)
        {
            var tabs = await (from t in _context.TblTabs
                              join p in _context.TblPermissions on t.TabId equals p.TabId
                              where p.RoleId == roleId
                              select new SidebarModel
                              {
                                  TabId = t.TabId,
                                  TabName = t.TabName,
                                  ParentId = t.ParentId,
                                  TabUrl = t.TabUrl,
                                  IconPath = t.IconPath
                                  //IsActive = t.IsActive
                              }).ToListAsync();

            // Group the tabs into a hierarchical structure (parent-child)
            var tabHierarchy = tabs
                .Where(tab => tab.ParentId == null)
                .Select(tab => new SidebarModel
                {
                    TabId = tab.TabId,
                    TabName = tab.TabName,
                    ParentId = tab.ParentId,
                    TabUrl = tab.TabUrl,
                    IconPath = tab.IconPath,
                    //IsActive = tab.IsActive,
                    SubTabs = tabs.Where(sub => sub.ParentId == tab.TabId).ToList()
                }).ToList();

            return tabHierarchy;
        }

    }
}
