using DocumentFormat.OpenXml.Office2010.Excel;
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
        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
        //    var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result; // Sync for simplicity
        //    ViewBag.SidebarTabs = tabs;
        //    base.OnActionExecuting(context);
        //}
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Prevent caching
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            //var httpContext = context.HttpContext;

            // Fetch session values
            int? userId = HttpContext.Session.GetInt32("MemberId");
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            // ✅ Ensure at least one valid login session exists
            if (userId == null || roleId == null)
            {
                context.Result = new RedirectToActionResult("login", "library", null);
                return;
            }

            // ✅ Only fetch sidebar tabs if roleId is valid
            if (roleId > 0)
            {
                var tabs = await _sidebar.GetTabsByRoleIdAsync(roleId.Value); // Async call
                ViewBag.SidebarTabs = tabs;
                var member = _sidebar.GetMember((int)userId);
                ViewBag.memberdetails = member;
            }

            await next(); // Continue with the action execution
        }
    }
}
