using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace library_management.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ISidebarRepository _sidebar;
        public BaseController(ISidebarRepository sidebar)
        {
            _sidebar = sidebar;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result; // Sync for simplicity
            ViewBag.SidebarTabs = tabs;
            base.OnActionExecuting(context);
        }
    }
}
