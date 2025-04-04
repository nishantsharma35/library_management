using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace library_management.Controllers
{
    public class MembershipController : Controller
    {
        private readonly dbConnect _connect;
        private readonly MembershipInterface _membershipInterface;
        private readonly PermisionHelperInterface _permission;
        public MembershipController(MembershipInterface membershipInterface, dbConnect connect, PermisionHelperInterface permission)
        {
            _membershipInterface = membershipInterface;
            _connect = connect;
            _permission = permission;
        }

        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }
        public IActionResult Index()
        {
            return View();
        }


        


        [HttpGet]
        public async Task<IActionResult> Joinlibrary()
        {
            string permissionType = GetUserPermission("MemberSite");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                var libraries = await _connect.Libraries.ToListAsync();
                return View(libraries);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }






        [HttpPost]
        public async Task<IActionResult> JoinLibrary(Membership model)
        {
            try
            {
                var memberEmail = HttpContext.Session.GetString("UserEmail");
                var member = await _connect.Members.FirstOrDefaultAsync(m => m.Email == memberEmail);

                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found!" });
                }

                var membership = new Membership
                {
                    MemberId = member.Id,
                    LibraryId = model.LibraryId,
                    IsActive = false,
                    IsDeleted = false
                };

                await _connect.Memberships.AddAsync(membership);
                await _connect.SaveChangesAsync();

                return Json(new { success = true, message = "Membership request sent. Please wait for admin approval." });


            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

    }
}
