using DocumentFormat.OpenXml.Spreadsheet;
using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol.Plugins;
using System.Net.Http;
using System.Security.Claims;
using library_management.DTO;

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
        private readonly HttpClient _httpClient;
        public libraryController(dbConnect db , libraryInterface libraryInterface, EmailSenderInterface emailSender, loginInterface login,IMemoryCache memoryCache,PermisionHelperInterface permisionHelperInterface, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _libraryInterface = libraryInterface;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
            _permission = permisionHelperInterface;
            _httpClient = httpClientFactory.CreateClient();
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
                _memoryCache.Remove(attemptKey);
                return Json(new { success = true, message = "Login successful!" , res = RedirectTo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("/library/states")]
        public async Task<IActionResult> GetStates()
        {
            var response = await _httpClient.GetStringAsync("http://api.geonames.org/childrenJSON?geonameId=1269750&username=nishant_35");
            return Content(response, "application/json");
        }

        [HttpGet("/library/cities/{geonameId}")]
        public async Task<IActionResult> GetCities(int geonameId)
        {
            var url = $"http://api.geonames.org/childrenJSON?geonameId={geonameId}&username=nishant_35";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
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
                HttpContext.Session.SetInt32("UserRoleId", model.RoleId);
                var result = await _libraryInterface.AddmemberAsync(model);
                HttpContext.Session.SetInt32("MemberId", model.Id);
                return Json(result);

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
        public async Task<IActionResult> Otpcheck([FromForm]Models.Member user)
        {
            try
            {
                if (await _libraryInterface.isemailexitsAsync(user.Email))
                {
                    if (await _libraryInterface.OtpVerification(user.Otp))
                    {
                        await _libraryInterface.updateStatus(user.Email);
                        int id =  HttpContext.Session.GetInt32("UserRoleId") ?? 0 ;

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
                return Json(new { success = false, message = "Unknown error occurred", error = e.Message });
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
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid email address.");
            }

            HttpContext.Session.SetString("ForgotPassEmail", user.Email);
            var res = await _login.TokenSenderViaEmail(user.Email);

            if (res == null)
            {
                return BadRequest("Error in sending email.");
            }

            return Ok(new { message = "Password reset link has been sent to your email." });
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

                //if(result == null)
                //{
                //    return Json(new { success = false, message ="Something went wrong." });
                //}
                //else
                //{
                //    return Json(new { success = true, message = "✅ Librarian added successfully!" });
                //}


                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public IActionResult GoogleDetails(string email)
        {
            var model = new GoogleSignupModel { Email = email };
            return View(model);
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "library");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("library/google-callback-view")]
        public IActionResult GoogleCallback()
        {
            return View("GoogleCallback", new object()); // This is your Razor view with JS
        }

        [HttpGet("library/google-callback")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return Json(new { success = false, message = "Google authentication failed. Please try again." });
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity.Name;

            var user = await _db.Members.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && user.IsGoogleAccount == true)
            {
                // ❌ Disallow Librarian
                if (user.RoleId == 4)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Librarians are not allowed to login using Google. Please use email/password."
                    });
                }
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetInt32("UserRoleId", user.RoleId);
                HttpContext.Session.SetString("UserEmail", email);
                // ✅ Allow Super Admin
                if (user.RoleId == 1)
                {       
                    return Json(new
                    {
                        success = true,
                        message = "Super Admin login successful!",
                        redirect = "/Dashboard" // or your actual dashboard
                    });
                }

                // ✅ Allow Library Admin
                if (user.RoleId == 2)
                {
                    var library = await _db.Libraries.FirstOrDefaultAsync(x => x.AdminId == user.Id);
                    if (library == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No library is assigned to your account. please add one",
                            redirect = Url.Action("libraryRegistration", "library")
                        });
                    }

                    if (user.VerificationStatus == "Rejected")
                    {
                        return Json(new { success = false, message = "Your account has been rejected." });
                    }

                    if (user.VerificationStatus == "Pending")
                    {
                        return Json(new { success = false, message = "Your account is pending approval." });
                    }

                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetInt32("UserRoleId", user.RoleId);
                    HttpContext.Session.SetInt32("LibraryId", library.LibraryId);

                    return Json(new
                    {
                        success = true,
                        message = "Library Admin login successful!",
                        redirect = Url.Action("index", "Dashboard")
                    });
                }

                // ✅ Allow Member
                if (user.RoleId == 3)
                {
                    

                    return Json(new
                    {
                        success = true,
                        message = "Member login successful!",
                        redirect = Url.Action("index", "Dashboard") // update as needed
                    });
                }

                // ❌ Unknown Role
                return Json(new { success = false, message = "Unauthorized role. Contact support." });
            }
            // New Google user
            return Json(new
            {
                success = true,
                message = "Google account not found. Please complete your details.",
                redirect = Url.Action("GoogleDetails", "library", new { email })
            });
        }


        [HttpPost]
        public async Task<IActionResult> GoogleDetails(GoogleSignupModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please fill all required fields." });

            // Check if username already exists
            bool usernameExists = _db.Members.Any(u => u.Name == model.Username);
            if (usernameExists)
            {
                return Json(new { success = false, message = "Username Already Exists" });
            }

            // Create temp password
            string tempPassword = Guid.NewGuid().ToString();

            // Create user
            var user = new Models.Member
            {
                Email = model.Email,
                Name = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                IsGoogleAccount = true,
                IsEmailVerified = true,
                RoleId = model.RoleId,
                VerificationStatus = "Pending"
            };

            _db.Members.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("UserRoleId", user.RoleId);

            // Admin = 2, Member = 3
            if (model.RoleId == 2)
            {
                return Json(new
                {
                    success = true,
                    message = "Redirecting to Library Registration...",
                    redirect = Url.Action("libraryRegistration", "Library")
                });
            }

            // Member redirect
            TempData["google-toast"] = "Account Created Successfully using Google!";
            TempData["google-toastType"] = "success";

            return Json(new { success = true, message = "Account Created", redirect = Url.Action("", "Dashboard") });
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "library");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.oldPassword) || string.IsNullOrWhiteSpace(model.newPassword))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            int userId = HttpContext.Session.GetInt32("UserId").Value; // Get logged-in user ID
            var user = await _db.Members.FindAsync(userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.oldPassword, user.Password))
            {
                return BadRequest(new { success = false, message = "Old password is incorrect." });
            }

            if (model.newPassword.Length < 8)
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.newPassword);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully." });
        }
    }
}
