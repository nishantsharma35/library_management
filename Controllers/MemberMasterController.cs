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
        public MemberMasterController(dbConnect context, EmailSenderInterface emailSender, ILogger<MemberMasterController> logger, ISidebarRepository sidebar, MemberMasterInterface memberMasterInterface , libraryInterface libraryInterface) : base(sidebar) 
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
            _memberMasterInterface = memberMasterInterface;
            _libraryInterface = libraryInterface;
        }
        

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MemberList()
        {
            //var pendingUsers = await _context.TblUsers.Where(u => u.VerificationStatus == "Pending").ToListAsync();
            //return View(pendingUsers);

            return View(await _memberMasterInterface.GetAllAdminsData());

            //var users = _context.TblUsers
            //            .Where(u => !u.IsApproved)
            //            .Select(u => new SidebarModel
            //            {
            //                UserId = u.UserId,
            //                Username = u.Username,
            //                Email = u.Email,
            //                RegistrationDate = u.RegistrationDate
            //            })
            //            .ToList();

            //return View(users);
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
            var roleName = _context.tblRoles
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



        //[Route("Admins/Save")]
        [HttpGet]
        public async Task<IActionResult> AddMember(int? id)
        {
            Member model = new Member();
            if (id > 0)
            {
                model = await _context.Members.FirstOrDefaultAsync(x => x.Id == id);
            }
            return View(model);
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
                string join = user.Joiningdate.ToString();


                int id = user.Id;

                //int ResId = (int)HttpContext.Session.GetInt32("UserId");
                //string verifier = ResId.ToString();
                var res = await _memberMasterInterface.AddMember(user);

                if (id > 0)
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Admin account has been updated by the SuperAdmin";
                        string body = $"Hello there {user.Name}, your account has been successfully update by the SuperAdmin and you can access your account now, some information has been update and your username and email has been mailed regardless of any changes, you can now login under the given credentials. UserName : {user.Name}, Email : {user.Email}, Password : you old same pass. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                else
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Admin account has been created by the SuperAdmin";
                        string body = $"Hello there {username}, welcome to our WMS Application, you account has been successfully created and you can access your account under the given credentials. UserName : {username}, Email : {email}, Password : {password}, Phone : {phone}, Joining Date : {join} and after login, you can fill out your extra informations. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
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

        [HttpPost]
        //[ValidateAntiForgeryToken] // Protect against CSRF attacks
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

                _context.Members.Remove(user);
                _context.SaveChanges();

                return Json(new { success = true, message = "user deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return Json(new { success = false, message = "Error deleting user." });
            }
        }







    }
}
