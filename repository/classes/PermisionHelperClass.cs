using library_management.Models;
using library_management.repository.internalinterface;

namespace library_management.repository.classes
{
    public class PermisionHelperClass : PermisionHelperInterface
    {
        private readonly dbConnect _context;

        public PermisionHelperClass(dbConnect context)
        {
            _context = context;
        }
        public string HasAccess(string tabName, int roleId)
        {
            var currentTabId = _context.TblTabs.Where(x => x.TabName == tabName).Select(y => y.TabId).FirstOrDefault();
            var currentTabPermissions = GetTabByRole(roleId, currentTabId);
            return currentTabPermissions?.PermissionType;
        }
        public SidebarModel GetTabByRole(int roleId, int currentTabId)
        {
            return _context.TblPermissions.Where(x => x.RoleId == roleId && x.IsActive == true && x.TabId == currentTabId).Select(y => new SidebarModel
            {
                TabId = y.TabId,
                IsActive = y.IsActive,
                PermissionType = y.PermissionType
            }).FirstOrDefault();
        }
    }
}
