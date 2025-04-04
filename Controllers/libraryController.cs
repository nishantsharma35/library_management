using DocumentFormat.OpenXml.Spreadsheet;
using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol.Plugins;

namespace library_management.Controllers
{
    public class libraryController : Controller
    {
        private readonly dbConnect _db;
        private readonly libraryInterface _libraryInterface;
        private readonly EmailSenderInterface _emailSender;
        private readonly loginInterface _login;
        private  readonly IMemoryCache _memoryCache;
        private readonly PermisionHelperInterface _permission;
        public libraryController(dbConnect db , libraryInterface libraryInterface, EmailSenderInterface emailSender, loginInterface login,IMemoryCache memoryCache,PermisionHelperInterface permisionHelperInterface)
        {
            _db = db;
            _libraryInterface = libraryInterface;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
            _permission = permisionHelperInterface;
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

        public async Task<IActionResult> Register()
        {
                return View();
        }


        public IActionResult login()
        {
            var model = new LoginModel();
            if (Request.Cookies.TryGetValue("RememberMe_Email", out string Emailvalue))
            {
                model.EmailOrName = Emailvalue;
                model.RememberMe = true;
            }

            

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            string RedirectTo = "Dashboard";


            try
            {
                const int maxAttempts = 5; // Maximum allowed attempts
                const int lockoutDurationSeconds = 120; // Lockout duration in seconds

                // Define cache keys for tracking attempts and lockout status
                var attemptKey = $"LoginAttempts_{login.EmailOrName}";
                var lockoutKey = $"Lockout_{login.EmailOrName}";

                // Check if the user is locked out
                if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
                {
                    var remainingTime = (lockoutEndTime - DateTime.Now).Seconds;
                    return Json(new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." });
                }

                // Authenticate user using the interface
                var result = await _login.AuthenticateUser(login.EmailOrName, login.Password);

                // Handle unsuccessful login
                if (!((dynamic)result).success)
                {
                    // Increment the login attempts
                    var attempts = _memoryCache.GetOrCreate(attemptKey, entry => 0) + 1;
                    _memoryCache.Set(attemptKey, attempts, TimeSpan.FromSeconds(lockoutDurationSeconds));

                    // Calculate remaining attempts
                    var remainingAttempts = maxAttempts - attempts;

                    // Lockout the user if max attempts reached
                    if (attempts >= maxAttempts)
                    {
                        _memoryCache.Set(lockoutKey, DateTime.Now.AddSeconds(lockoutDurationSeconds), TimeSpan.FromSeconds(lockoutDurationSeconds));
                        _memoryCache.Remove(attemptKey); // Clear attempts after locking out
                        return Json(new { success = false, message = $"Account is locked. Try again in {lockoutDurationSeconds} seconds." });
                    }

                    // Return the error message from AuthenticateUser along with remaining attempts
                    return Json(new { success = false, message = $"{((dynamic)result).message} You have {remainingAttempts} attempts left." });
                }

                // Successful login
                _memoryCache.Remove(attemptKey);

                string email = await _libraryInterface.fetchEmail(login.EmailOrName);

                var data = await _libraryInterface.GetUserDataByEmail(email);
                var member = _db.Members.FirstOrDefault(m => m.Email == email);
                if (member?.RoleId == 2) // admin Role
                {
                    var librarian = _db.Libraries.FirstOrDefault(l => l.AdminId == member.Id);
                    if (librarian != null)
                    {
                         HttpContext.Session.SetInt32("LibraryId", librarian.LibraryId); // ✅ Int value set karo
                       
                    }
                }
                else if(member.RoleId == 4) // admin Role
                {
                    
                        HttpContext.Session.SetInt32("LibraryId",(int) member.LibraryId); // ✅ Int value set karo
                    
                }

                HttpContext.Session.SetInt32("MemberId", data.Id);
                // ✅ Har kisi ka RoleId bhi session me store karo
                HttpContext.Session.SetInt32("UserRoleId", data.RoleId);


                //if (member?.RoleId == 2) // Direct Role Check
                //{
                //    var librarian = _db.Libraries.FirstOrDefault(l => l.AdminId == member.Id);
                //    if (librarian != null)
                //    {
                //        HttpContext.Session.SetString("LibraryId", librarian.LibraryId.ToString());
                //    }
                //}

                //// Clear login attempts
                HttpContext.Session.SetString("Cred", login.EmailOrName);

                // Remember Me functionality
                if (login.RememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7), // Cookie expiration time
                        HttpOnly = true,                  // For security
                        Secure = true                     // Use HTTPS
                    };
                    Response.Cookies.Append("RememberMe_Email", login.EmailOrName, options);
                    Response.Cookies.Append("RememberMe_Password", login.Password, options);
                }
                else
                {
                    // Clear cookies if Remember Me is not checked
                    Response.Cookies.Delete("RememberMe_Email");
                    Response.Cookies.Delete("RememberMe_Password");
                }


                string Email = await _libraryInterface.fetchEmail(login.EmailOrName);
                HttpContext.Session.SetString("UserEmail", Email);

                var Data = await _libraryInterface.GetUserDataByEmail(email);
                int id = data.Id;
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetInt32("UserRoleId", Data.RoleId);

                if (data.RoleId != 1)
                {
                    if (await _libraryInterface.IsVerified(login.EmailOrName))
                    {
                        if (data.RoleId == 2)
                        {
                            if (!await _libraryInterface.GetAdmindetails(id))
                            {
                                    RedirectTo = "libraryRegistration";
                            }
                        }
                    }
                    else
                    {
                        RedirectTo = "Otpcheck"; // Redirect to OTP verification if not verified
                    }
                }

                if (RedirectTo == "Otpcheck")
                {
                    var user = await _db.Members.FirstOrDefaultAsync(x => x.Email == email);
                    if (user != null)
                    {
                        user.Otp = _emailSender.GenerateOtp();
                        user.Otpexpiry = DateTime.Now.AddMinutes(5);
                        await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp);
                        await _db.SaveChangesAsync();
                    }

                }

                // Reset login attempts after successful login
                _memoryCache.Remove(attemptKey);
                return Json(new { success = true, message = "Login successful!" , res = RedirectTo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Register([FromForm] Models.Member model)
        {
            try
            {
                if (await _libraryInterface.ismembernameexitsAsync(model.Name))
                {
                    return Json(new { success = false, message = "Membername Already Exits" });
                }
                if (await _libraryInterface.isemailexitsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Email Already Exits" });
                }
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetInt32("RoleId", model.RoleId);
                return Json(await _libraryInterface.AddmemberAsync(model));
            }
            catch (Exception)
            {

                throw;
            }
            //        try
            //{
            //    // Check if member name already exists
            //    if (await _libraryInterface.ismembernameexitsAsync(model.Name))
            //    {
            //        return Json(new { success = false, message = "Member name already exists" });
            //    }

            //    // Check if email already exists
            //    if (await _libraryInterface.isemailexitsAsync(model.Email))
            //    {
            //        return Json(new { success = false, message = "Email already exists" });
            //    }

            //    // Register the member
            //    var result = await _libraryInterface.AddmemberAsync(model);

            //    if ((bool)result)
            //    {
            //        // Set session values only if registration is successful
            //        HttpContext.Session.SetString("UserEmail", model.Email);
            //        HttpContext.Session.SetInt32("RoleId", model.RoleId);
            //    }

            //    return Json(new {success = false,message = "Registered Successfully" }, result);
            //}
            //catch (Exception ex)
            //{
            //    // Log error for debugging
            //    Console.WriteLine($"Error in Register method: {ex.Message}");

            //    return Json(new { success = false, message = "An error occurred while processing your request." });
            //}

        }
        public IActionResult libraryRegistration()
        {
                return View();
        }
        [HttpPost]
        public async Task<IActionResult> libraryRegistration( Library lib , int id)
        {
            try
             {
                string va = HttpContext.Session.GetString("UserEmail");
                 id = (int) await _libraryInterface.GetUserIdByEmail(va);

                return Json(await _libraryInterface.AddLibraryAsync(lib, id));
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IActionResult> TestEmail()
        {
            try
            {
                // Replace with your recipient email address
                string recipientEmail = "mailto:nishantt.visitorz@gmail.com";
                string subject = "Test Email";
                string body = "<p>This is a test email sent from your application.</p>";

                await _emailSender.SendEmailAsync(recipientEmail, subject, body);

                return Ok("Test email sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

        public IActionResult Otpcheck()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Otpcheck(Models.Member user)
        {
            try
            {
                if (await _libraryInterface.isemailexitsAsync(user.Email))
                {
                    if (await _libraryInterface.OtpVerification(user.Otp))
                    {
                        await _libraryInterface.updateStatus(user.Email);
                        int id = (int) HttpContext.Session.GetInt32("UserRoleId");

                        string adminEmail = "nishantsharma3637@gmail.com";
                        string subject = "New User Registration - Pending Approval";
                        string resetUrl = "http://localhost:5041/MemberMaster/MemberList";
                        string body = $"User: {user.Name}<br>Email: {user.Email}<br>Date: {DateTime.UtcNow}<br>Click <a href='{resetUrl}'>here</a> to go to admin panel.";

                        await _emailSender.SendEmailAsync(adminEmail, subject, body);

                        return Json(new { success = true, message = "OTP verified successfully", id });

                    }
                    else
                    {
                        return Json(new { success = false, message = "OTP verification failed" });
                    }
                }
                return Json(new { success = false, message = "Email not found" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });

            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                var user = await _db.Members.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.Otpexpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp);
                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }

        public IActionResult ForgotPassword()
        {
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(Models.Member user)
        {
            HttpContext.Session.SetString("ForgotPassEmail", user.Email);
            var res = await _login.TokenSenderViaEmail(user.Email);
            return Ok(res);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
                // Validate the token
                if (string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out object tokenData))
                {
                    ViewBag.InvalidToken = true;
                    return View();
                    //return Json(new { success = false, message = "Token is Invalid or Expired, please send another link" });
                }
                ViewBag.InvalidToken = false;
                ViewBag.Token = token;
                // Remove the token from cache after successful validation
                _memoryCache.Remove(token);

                return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAction(string Password)
        {
            string user = HttpContext.Session.GetString("ForgotPassEmail");
            var res = await _login.ResetPassword(user, Password);

            return Ok(res);
        }

        [HttpGet]
        public IActionResult AddLibrarian()
        {
            string permissionType = GetUserPermission("Add Librarian");
            if (permissionType == "CanView" || permissionType == "CanEdit" || permissionType == "FullAccess")
            {
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLibrarian(Models.Member librarian)
        {
            try
            {
                // ✅ Get Admin's LibraryId from session
                int? adminLibraryId = HttpContext.Session.GetInt32("LibraryId");
                if (adminLibraryId == null)
                {
                    return Json(new { success = false, message = "Admin's library not found." });
                }

                // ✅ Call repository method
                var result = await _libraryInterface.AddLibrarianAsync(librarian, adminLibraryId.Value);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

    }
}
