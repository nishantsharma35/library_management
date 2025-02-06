using library_management.Models;
using library_management.Repositories.Classes;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace library_management.repository.classes
{
    public class loginClass : loginInterface
    {
        private readonly dbConnect _connect;
        private readonly EmailSenderInterface _emailSenderClass;
        private readonly IMemoryCache _memoryCache;


        public loginClass(dbConnect connect, EmailSenderInterface emailSenderClass, IMemoryCache memoryCache)
        {
            _connect = connect;
            _emailSenderClass = emailSenderClass;
            _memoryCache = memoryCache;
        }


        public async Task<object> AuthenticateUser(string emailOrUsername, string password)
        {

            // Fetch user by email or username
            var user = await _connect.Members.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Name == emailOrUsername);
            Console.WriteLine($"Fetched user: {user?.Email}");

            if (user == null)
            {
                return new { success = false, message = "User not found " };
            }

            // Compare hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            Console.WriteLine($"Password valid: {isPasswordValid}");

            if (!isPasswordValid)
            {
                return new { success = false, message = "Invalid user id or pass" };
            }
            var lib = await _connect.Libraries.FirstOrDefaultAsync(u => u.LibraryId == user.Id);


            // Check VerificationStatus
            if(lib != null)
            {
                if (user.IsEmailVerified && user.VerificationStatus != "Accepted")
                {
                    string reason = user.VerificationStatus == "Pending"
                        ? "Your account is pending admin approval."
                        : "Your account has been declined by the admin.";
                    return new { success = false, message = reason };
                }
            }

            return new { success = true, message = "You are successfully logged in", email = user.Email };
        }

        //This method will send token to user's email for password reset
        public async Task<object> TokenSenderViaEmail(string email)
        {
            //Fetching user by email
            var user = await _connect.Members.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new { success = false, message = "No user found with that email or username" };
            }

            //We are creating token variables here
            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddHours(1); // Token expires in 1 hour

            // Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { Id = user.Id, ExpirationTime = expirationTime }, expirationTime);

            // Create the password reset URL with the token
            var resetUrl = "http://localhost:5041/library/ResetPassword?token=" + resetToken;

            // Send the reset email
            await _emailSenderClass.SendEmailAsync(user.Email, "Password Reset Request",
                $"Click <a href='{resetUrl}'>here</a> to reset your password.");

            return new { success = true, message = "Password reset link sent to your email." };
        }

        //This method will reset password of user
        public async Task<object> ResetPassword(string creds, string newPassword)
        {
            //_memoryCache.Remove(token);

            var user = _connect.Members.FirstOrDefault(u => u.Email == creds || u.Name == creds);
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _connect.SaveChanges();
                return new { success = true, message = "Password Changed Successfully" };

            }

            return new { success = false, message = "User not found" };
        }
    }
        
}
