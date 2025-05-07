using DocumentFormat.OpenXml.Spreadsheet;
using library_management.Models;
using library_management.repository.classes;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Net.Http;

namespace library_management.Controllers
{
    public class AdminMasterController : BaseController
    {
        private readonly dbConnect _connect;
        private readonly EmailSenderInterface _sender;
        private readonly ILogger<AdminMasterController> _logger;
        private readonly AdminInterface _adminInterface;
        private readonly libraryInterface _libraryInterface;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PermisionHelperInterface _permission;
        private readonly HttpClient _httpClient;
        private readonly IActivityRepository _activityRepository;
        public AdminMasterController(dbConnect connect,EmailSenderInterface sender,ISidebarRepository sidebar,ILogger<AdminMasterController> logger,AdminInterface admin,libraryInterface libraryInterface,IWebHostEnvironment webHostEnvironment,PermisionHelperInterface permisionHelperInterface, IHttpClientFactory httpClientFactory,IActivityRepository activityRepository) : base(sidebar)
        {
            _connect = connect;
            _sender = sender;
            _logger = logger;
            _adminInterface = admin;
            _libraryInterface = libraryInterface;
            _webHostEnvironment = webHostEnvironment;
            _permission = permisionHelperInterface;
            _httpClient = httpClientFactory.CreateClient();
            _activityRepository = activityRepository;
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

        public async Task<IActionResult> AdminList()
        {
            string permissionType = GetUserPermission("AdminList");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                return View(await _adminInterface.GetAllAdminsData());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public IActionResult AdminDetails(int id)
        {
            string permissionType = GetUserPermission("AdminList");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                Library admin = _connect.Libraries.FirstOrDefault(x => x.LibraryId == id);
                return View(admin);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


        [HttpGet("/AdminMaster/states")]
        public async Task<IActionResult> GetStates()
        {
            var response = await _httpClient.GetStringAsync("http://api.geonames.org/childrenJSON?geonameId=1269750&username=nishant_35");
            return Content(response, "application/json");
        }

        [HttpGet("/AdminMaster/cities/{geonameId}")]
        public async Task<IActionResult> GetCities(int geonameId)
        {
            var url = $"http://api.geonames.org/childrenJSON?geonameId={geonameId}&username=nishant_35";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
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
            //string permissionType = GetUserPermission("AddAdmin");
            //if (permissionType == "CanEdit" || permissionType == "FullAccess" || permissionType == "CanView")
            //{
                Library model = new Library();
                if (id > 0)
                {
                    model = await _connect.Libraries.FirstOrDefaultAsync(x => x.LibraryId == id);
                }
                return View(model);
            //}
            //else
            //{
            //    return RedirectToAction("UnauthorisedAccess", "Error");
            //}
            
        }


        [HttpPost]
        public async Task<IActionResult> AddAdmin(Library user, IFormFile LibraryFile)
        {
            int uid = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _connect.Members.Where(x => x.Id == uid).Select(y => y.Name).FirstOrDefault();
            string libName = _connect.Libraries.Where(x => x.AdminId == uid).Select(y => y.Libraryname).FirstOrDefault();

            try
            {
                bool isUpdate = user.AdminId > 0;

                // 🧠 1. Validation Check
                if (isUpdate)
                {
                    // Update Mode: Check for duplicate name with different ID
                    Library existingLibrary = _adminInterface.checkExistence(user.Libraryname, user.LibraryId);
                    //if (existingLibrary != null && existingLibrary.Libraryname == user.Libraryname)
                    //{
                    //    return Json(new { success = false, message = "Library name already exists" });
                    //}
                }
                else
                {
                    // Add Mode: Ensure new library name is unique
                    if (await _libraryInterface.ismembernameexitsAsync(user.Libraryname))
                    {
                        return Json(new { success = false, message = "Library name already exists" });
                    }
                }

                // 🖼️ 2. Handle Image Upload
                if (LibraryFile != null && LibraryFile.Length > 0)
                {
                    // Delete old image (if updating)
                    if (!string.IsNullOrEmpty(user.LibraryImagePath))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", user.LibraryImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(LibraryFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LibraryFile.CopyToAsync(stream);
                    }

                    user.LibraryImagePath = fileName;
                }

                // 💾 3. Save to Database (Add or Update)
                var result = await _adminInterface.AddAdmin(user);

                // 📝 4. Activity Log
                string activityType = isUpdate ? "updated admin details" : "added admin details";
                string description = $"Superadmin {activityType} for {user.Libraryname}";
                _activityRepository.AddNewActivity(uid, activityType, description);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }





        //[HttpPost]
        //public async Task<IActionResult> AddAdmin(Library user, IFormFile LibraryFile)
        //{
        //    int uid = (int)HttpContext.Session.GetInt32("UserId");
        //    string userName = _connect.Members.Where(x => x.Id == uid).Select(y => y.Name).FirstOrDefault();
        //    string libName = _connect.Libraries.Where(x => x.AdminId == uid).Select(y => y.Libraryname).FirstOrDefault();

        //    try
        //    {
        //        // ✅ Check if Library already exists
        //        if (user.AdminId > 0)
        //        {
        //            Library resData = _adminInterface.checkExistence(user.Libraryname, user.LibraryId);
        //            if (resData != null && resData.Libraryname == user.Libraryname)
        //            {
        //                return Json(new { success = false, message = "Library name already exists" });
        //            }
        //        }
        //        else
        //        {
        //            if (await _libraryInterface.ismembernameexitsAsync(user.Libraryname))
        //            {
        //                return Json(new { success = false, message = "Username already exists - edit" });
        //            }
        //        }

        //        // ✅ Fetching values
        //        string username = user.Libraryname;
        //        string address = user.Address;
        //        int AdminId = Convert.ToInt32(user.AdminId);
        //        string startTime = user.StartTime?.ToString() ?? "";
        //        string closingTime = user.ClosingTime?.ToString() ?? "";
        //        int Pincode = Convert.ToInt32(user.Pincode);
        //        string state = user.State;
        //        string city = user.City;
        //        int id = user.LibraryId;

        //        // ✅ Image Upload Handling
        //        if (LibraryFile != null && LibraryFile.Length > 0)
        //        {
        //            // Agar purani image hai to delete karo
        //            if (!string.IsNullOrEmpty(user.LibraryImagePath))
        //            {
        //                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", user.LibraryImagePath);
        //                if (System.IO.File.Exists(oldImagePath))
        //                {
        //                    System.IO.File.Delete(oldImagePath);
        //                }
        //            }

        //            // Naya image save karo
        //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(LibraryFile.FileName);
        //            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

        //            if (!Directory.Exists(uploadPath))
        //            {
        //                Directory.CreateDirectory(uploadPath);
        //            }

        //            string filePath = Path.Combine(uploadPath, fileName);
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await LibraryFile.CopyToAsync(stream);
        //            }

        //            // Database me image ka naam save karo
        //            user.LibraryImagePath = fileName;
        //        }

        //        // ✅ Save Library Data
        //        var res = await _adminInterface.AddAdmin(user);

        //        string type = "added admin details";
        //        string desc = $"Superadmin added admin details for {libName}";
        //        _activityRepository.AddNewActivity(uid, type, desc);

        //        return Ok(res);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return Json(new { success = false, message = "Unknown error occurred" });
        //    }
        //}


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
        [HttpGet]
        public async Task<IActionResult> PendingMemberships()
        {
            string permissionType = GetUserPermission("Manage Members");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                try
                {
                    // Check if the session has the admin email
                    var adminEmail = HttpContext.Session.GetString("UserEmail");

                    if (string.IsNullOrEmpty(adminEmail))
                    {
                        Console.WriteLine("Session doesn't contain admin email.");
                        return RedirectToAction("Login"); // If no admin email in session, redirect to login
                    }

                    // Get admin info from the database (make sure this user is an admin)
                    var admin = await _connect.Members
                        .FirstOrDefaultAsync(m => m.Email == adminEmail && m.RoleId == 2);

                    if (admin == null)
                    {
                        Console.WriteLine($"Admin not found or user is not an admin. Email: {adminEmail}");
                        return RedirectToAction("Login"); // Redirect if the user is not an admin
                    }

                    // Log admin info to confirm it's retrieved correctly
                    Console.WriteLine($"Admin found: {admin.Email}, Admin ID: {admin.Id}");

                    // Query for pending memberships for this admin's library
                    var pendingMembershipsQuery = _connect.Memberships
                        .Include(m => m.Member)
                        .Include(m => m.Library)
                        .Where(m => m.Library.AdminId == admin.Id && m.IsActive == false && m.IsDeleted == false);

                    // Check if the query is returning any data before calling ToListAsync
                    var pendingMemberships = await pendingMembershipsQuery.ToListAsync();

                    if (pendingMemberships == null || !pendingMemberships.Any())
                    {
                        Console.WriteLine("No pending memberships found for this admin.");
                        return View(new List<Membership>()); // Return empty if no pending memberships
                    }

                    return View(pendingMemberships);
                }
                catch (Exception ex)
                {
                    // Log any errors that occur
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    return View(new List<Membership>());
                }

            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveMembership(int id)
        {
            int userid = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _connect.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _connect.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var membership = await _connect.Memberships.FindAsync(id);
            if (membership == null)
            {
                return Json(new { success = false, message = "Membership not found!" });
            }

            membership.IsActive = true;
            await _connect.SaveChangesAsync();

            string type = "approved member";
            string desc = $"{userName} approved membership for {libName}";
            _activityRepository.AddNewActivity(id, type, desc);

            return Json(new { success = true, message = "Membership approved successfully!" });
        }
        [HttpPost]
        public async Task<IActionResult> RejectMembership(int id)
        {
            int userid = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _connect.Members.Where(x => x.Id == id).Select(y => y.Name).FirstOrDefault();
            string libName = _connect.Libraries.Where(x => x.AdminId == id).Select(y => y.Libraryname).FirstOrDefault();

            var membership = await _connect.Memberships.FindAsync(id);
            if (membership == null)
            {
                return Json(new { success = false, message = "Membership not found!" });
            }

            membership.IsDeleted = true; // Mark as rejected
            await _connect.SaveChangesAsync();

            string type = "reject member";
            string desc = $"{userName} reject membership for {libName}";
            _activityRepository.AddNewActivity(id, type, desc);

            return Json(new { success = true, message = "Membership rejected successfully!" });
        }

      
    }
}
