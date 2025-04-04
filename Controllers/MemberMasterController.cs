using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace library_management.Controllers
{
    public class MemberMasterController : BaseController
    {
        private readonly dbConnect _context;
        private readonly EmailSenderInterface _emailSender;
        private readonly ILogger<MemberMasterController> _logger;
        private readonly MemberMasterInterface _memberMasterInterface;
        private readonly libraryInterface _libraryInterface;
        private readonly PermisionHelperInterface _permission;
        public MemberMasterController(dbConnect context, EmailSenderInterface emailSender, ILogger<MemberMasterController> logger, ISidebarRepository sidebar, MemberMasterInterface memberMasterInterface , libraryInterface libraryInterface,PermisionHelperInterface permision) : base(sidebar) 
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
            _memberMasterInterface = memberMasterInterface;
            _libraryInterface = libraryInterface;
            _permission = permision;
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

        public async Task<IActionResult> MemberList()
        {
            string permissionType = GetUserPermission("View Member");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                try
                {
                    int? userId = HttpContext.Session.GetInt32("UserId");
                    int? userRoleId = HttpContext.Session.GetInt32("UserRoleId"); // ✅ Role Check ke liye

                    if (userId == null || userId == 0)
                    {
                        ViewBag.ErrorMessage = "User ID is missing. Please log in again.";
                        return View("MemberList", new List<Member>());
                    }

                    List<Member> members;

                    if (userRoleId == 1) // ✅ Super Admin ho to sabhi members fetch karo
                    {
                        members = await _memberMasterInterface.GetAllMembersAsync();
                    }
                    else
                    {
                        var library = await _context.Libraries.FirstOrDefaultAsync(l => l.AdminId == userId.Value);
                        if (library == null)
                        {
                            ViewBag.ErrorMessage = "No associated library found for this admin.";
                            return View("MemberList", new List<Member>());
                        }

                        int libraryId = library.LibraryId;
                        members = await _memberMasterInterface.GetLibraryMembersAsync(libraryId);
                    }

                    return View("MemberList", members);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching members: {ex.Message}");
                    ViewBag.ErrorMessage = "An error occurred while fetching members.";
                    return View("MemberList", new List<Member>());
                }

            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> ApproveOrRejectUser(int userId, bool approve)
        {
            try
            {
                var user = await _context.Members.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return Json(new { success = false, message = "User not found." });

                user.VerificationStatus = approve ? "Accepted" : "Rejected";
                _context.Members.Update(user);
                await _context.SaveChangesAsync();

                var subject = approve ? "Account Accepted" : "Account Rejected";
                var body = approve
                    ? $"Hi {user.Name},<br>Your account is approved. Log in to start."
                    : $"Hi {user.Name},<br>Your account is rejected. Please contact support.";
                await _emailSender.SendEmailAsync(user.Email, subject, body);

                return Json(new { success = true, message = approve ? "User approved." : "User rejected." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
                return Json(new { success = false, message = "Error occurred." });
            }
        }
       
        [HttpGet]
        public IActionResult Details(int id)
        {
            var member = _context.Members.FirstOrDefault(m => m.Id == id);

            // Get Role Name
            var roleName = _context.TblRoles
                                   .Where(r => r.RoleId == member.RoleId)
                                   .Select(r => r.RoleName)
                                   .FirstOrDefault();

            ViewBag.RoleName = roleName;

            return View(member);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int UserId, bool Status)
        {
            var result = await _memberMasterInterface.UpdateStatus(UserId, Status);
            return Json(result);
        }



        [HttpGet]
        public async Task<IActionResult> AddMember(int? id)
        {
            string permissionType = GetUserPermission("Add Member");
            if (permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                Member model = new Member();
                if (id > 0)
                {
                    model = await _context.Members.FirstOrDefaultAsync(x => x.Id == id);
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
           
        }
        [HttpPost]
        public async Task<IActionResult> AddMember(Member user)
        {
            try
            {
                if (user.Id > 0)
                {
                    Member resData = _memberMasterInterface.checkExistence(user.Name, user.Email, user.Id);
                    if (resData != null)
                    {
                        if (resData.Email == user.Email)
                        {
                            return Json(new { success = false, message = "Email already exists" });
                        }
                        if (resData.Name == user.Name)
                        {
                            return Json(new { success = false, message = "Username already exists" });
                        }
                    }
                }
                else
                {
                    if (await _libraryInterface.ismembernameexitsAsync(user.Name))
                    {
                        return Json(new { success = false, message = "Username already exists - edit" });
                    }
                    if (await _libraryInterface.isemailexitsAsync(user.Email))
                    {
                        return Json(new { success = false, message = "Email already exists - edit" });
                    }
                }
                string username = user.Name;
                string address = user.Address;
                string password = user.Password;
                string email = user.Email;
                long phone = Convert.ToInt64(user.Phoneno);
                string gender = user.Gender;
                string state = user.State;
                string city = user.City;
                int roleid = user.RoleId;
                string join = user.Joiningdate.ToString();


                int id = user.Id;

                //int ResId = (int)HttpContext.Session.GetInt32("UserId");
                //string verifier = ResId.ToString();
                int? libraryId = HttpContext.Session.GetInt32("LibraryId");

                if (libraryId == null)
                {
                    return Json(new { success = false, message = "LibraryId not found in session." });
                }

                var res = await _memberMasterInterface.AddMember(user, libraryId.Value);
                //var res = await _memberMasterInterface.AddMember(user);


                if (id > 0)
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Member account has been updated by the Admin";
                        string body = $"Hello there {user.Name}, your account has been successfully update by the Admin and you can access your account now, some information has been update and your username and email has been mailed regardless of any changes, you can now login under the given credentials. UserName : {user.Name}, Email : {user.Email}, Password : you old same pass. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                else
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Member account has been created by the Admin";
                        string body = $"Hello there {username}, welcome to our LMS Application, you account has been successfully created and you can access your account under the given credentials. UserName : {username}, Email : {email}, Password : {password}, Phone : {phone}, Joining Date : {join} and after login, you can fill out your extra informations. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });
            }
        }

        //[ValidateAntiForgeryToken] // Protect against CSRF attacks
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid user id." });
            }

            try
            {
                var user = _context.Members.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if member has borrowed books
                var hasActiveBorrows = _context.Borrows.Any(b => b.MemberId == id && b.ReturnDate == null);
                var hasPendingFines = _context.Fines.Any(f => f.Borrow.MemberId == id && f.PaymentStatus != "Paid");

                if (hasActiveBorrows || hasPendingFines)
                {
                    return Json(new { success = false, message = "User cannot be deleted until all books are returned and fines are cleared." });
                }

                // Fetch membership and mark as deleted (soft delete)
                var membership = _context.Memberships.FirstOrDefault(m => m.MemberId == id);
                if (membership != null)
                {
                    membership.IsDeleted = true;
                    _context.SaveChanges();
                }

                return Json(new { success = true, message = "User membership deactivated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return Json(new { success = false, message = "Error deleting user." });
            }
        }
    }
}
