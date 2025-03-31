using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;

namespace library_management.Controllers
{
 
    public class MasterController : BaseController
    {
        private readonly dbConnect _context;

        public MasterController(dbConnect connect, ISidebarRepository sidebar) : base(sidebar)
        {
            _context = connect;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Access()
        {
            var tabs = _context.TblTabs.Where(x => x.IsActive == true).ToList();
            var roles = _context.TblRoles.Where(x => x.IsActive == true).ToList();

            ViewBag.Tabs = tabs;
            ViewBag.Roles = roles;
            return View(new List<TblPermission>());
        }

        [HttpGet]
        public JsonResult GetRolePermissions(int roleId)
        {
            var permissions = _context.TblPermissions
                .Where(p => p.RoleId == roleId)
                .Select(p => new {
                    p.PermissionId,
                    p.TabId,
                    TabName = _context.TblTabs.Where(t => t.TabId == p.TabId).Select(t => t.TabName).FirstOrDefault(),
                    p.PermissionType,
                    p.IsActive
                }).ToList();
            return Json(permissions);
        }

        [HttpPost]
        public IActionResult SavePermission([FromBody] TblPermission permission)
        {
            string msg = "";
            bool isSuccess = true;
            bool alreadyExists = _context.TblPermissions.Any(x => x.TabId == permission.TabId && x.RoleId == permission.RoleId);
            if (alreadyExists && permission.PermissionId == 0)
            {
                isSuccess = false;
                msg = "This Permission already exists you can edit it from the list";
            }
            else if (permission.PermissionId == 0)
            {
                _context.TblPermissions.Add(permission);
                msg = "Permission Added Successfully";
            }
            else
            {
                _context.TblPermissions.Update(permission);
                msg = "Permission Updated Successfully";
            }
            _context.SaveChanges();
            return Ok(new { success = isSuccess, message = msg });
        }

        [Route("Masters/DeletePermission")]
        [HttpGet]
        public IActionResult DeletePermission(int id)
        {
            var permission = _context.TblPermissions.Where(x => x.PermissionId == id).FirstOrDefault();
            if (permission == null)
            {
                return NotFound(); // Agar ID exist nahi karti
            }
            _context.TblPermissions.Remove(permission);
            _context.SaveChanges();
            return RedirectToAction("Access");
        }
    }
}
