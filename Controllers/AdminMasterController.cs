using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;

namespace library_management.Controllers
{
    public class AdminMasterController : BaseController
    {
        private readonly dbConnect _connect;
        private readonly EmailSenderInterface _sender;
        private readonly ILogger<AdminMasterController> _logger;
        private readonly AdminInterface _adminInterface;
        private readonly libraryInterface _libraryInterface;
        public AdminMasterController(dbConnect connect,EmailSenderInterface sender,ISidebarRepository sidebar,ILogger<AdminMasterController> logger,AdminInterface admin,libraryInterface libraryInterface) : base(sidebar)
        {
            _connect = connect;
            _sender = sender;
            _logger = logger;
            _adminInterface = admin;
            _libraryInterface = libraryInterface;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AdminList()
        {
            return View(await _adminInterface.GetAllAdminsData());
        }

        [HttpGet]
        public IActionResult AdminDetails(int id)
        {
            Library admin = _connect.Libraries.FirstOrDefault(x => x.LibraryId == id);

            return View(admin);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int UserId, bool Status)
        {
            var result = await _adminInterface.UpdateStatus(UserId, Status);
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> AddAdmin(int? id)
        {
            Library model = new Library();
            if (id > 0)
            {
                model = await _connect.Libraries.FirstOrDefaultAsync(x => x.LibraryId == id);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(Library user)
        {
            try
            {
                if (user.LibraryId > 0)
                {
                    Library resData = _adminInterface.checkExistence(user.Libraryname, user.LibraryId);
                    if (resData != null)
                    {
                        if (resData.Libraryname == user.Libraryname)
                        {
                            return Json(new { success = false, message = "Email already exists" });
                        }
                    }
                }
                else
                {
                    if (await _libraryInterface.ismembernameexitsAsync(user.Libraryname))
                    {
                        return Json(new { success = false, message = "Username already exists - edit" });
                    }
                }

                string username = user.Libraryname;
                string address = user.Address;
                int AdminId = Convert.ToInt32(user.AdminId);
                string starttime = user.StartTime.ToString();
                string closetime = user.ClosingTime.ToString();
                int Pincode = Convert.ToInt32(user.Pincode);
                string state = user.State;
                string city = user.City;
                //string join = user.CreatedAt.ToString();

                int id = user.LibraryId;

                var res = await _adminInterface.AddAdmin(user);

                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false, message = "Unknown error occurred" });
            }
        }


        [HttpPost]
        public IActionResult DeleteAdmin(int id)
        {
            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid user id." });
            }

            try
            {
                var user = _connect.Libraries.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                _connect.Libraries.Remove(user);
                _connect.SaveChanges();

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
