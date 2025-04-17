using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyModel;
using Mono.TextTemplating;
using System.Data;

namespace library_management.repository.classes
{
    public class librarymainClass : libraryInterface
    {
        private readonly dbConnect _connect;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailSenderInterface _emailSender;

        public librarymainClass(dbConnect connect , IWebHostEnvironment environment,EmailSenderInterface emailSender)
        {
            _connect = connect;
            _environment = environment;
            _emailSender = emailSender;
        }
        public async Task<object> AddmemberAsync(Member user) 
        {
            try
            {
                //member model details
                if (user.profilefile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    // Maximum allowed file size (5 MB in bytes)
                    const long maxFileSize = 5 * 1024 * 1024;

                    // Get file extension and check if it is allowed
                    string fileExtension = Path.GetExtension(user.profilefile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed." };
                    }

                    // Check file size
                    if (user.profilefile.Length > maxFileSize)
                    {
                        return new { success = false, message = "File size exceeds the maximum allowed size of 5 MB." };
                    }

                    string uploadFolder = Path.Combine(_environment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadFolder); // Ensure directory exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(user.profilefile.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await user.profilefile.CopyToAsync(stream);
                    }
                    user.Picture = "/images/" + uniqueFileName;
                }

                
                // Hash the password
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                // user.Confirmpassword = BCrypt.Net.BCrypt.HashPassword(user.Confirmpassword);
                user.Otp = _emailSender.GenerateOtp();
                user.Otpexpiry = DateTime.Now.AddMinutes(5);
                string subj = "OTP Verification!!";
                await _emailSender.SendEmailAsync(user.Email, subj, user.Otp);

                user.VerificationStatus = "Pending";

                // Save user to database
                await _connect.Members.AddAsync(user);
                await _connect.SaveChangesAsync();
                bool redirect = false;

                

                return new { success = true, message = "Registration successful. Please check your email to verify your account."};
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new { success = false, message = "An error occurred while adding the user." };
            }
        }



        public async Task<bool> IsVerified(string cred)
        {
            var user = await _connect.Members.FirstOrDefaultAsync(x => x.Email == cred || x.Name == cred);
            return user.IsEmailVerified;
        }

        public async Task<object> AddLibraryAsync(Models.Library library, int id)
        {
            try
            {
                //library model details
                if (library.libraryfile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    // Maximum allowed file size (5 MB in bytes)
                    const long maxFileSize = 5 * 1024 * 1024;

                    // Get file extension and check if it is allowed
                    string fileExtension = Path.GetExtension(library.libraryfile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed." };
                    }

                    // Check file size
                    if (library.libraryfile.Length > maxFileSize)
                    {
                        return new { success = false, message = "File size exceeds the maximum allowed size of 5 MB." };
                    }

                    string uploadFolder = Path.Combine(_environment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadFolder); // Ensure directory exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(library.libraryfile.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await library.libraryfile.CopyToAsync(stream);
                    }
                    library.LibraryImagePath = "/libraryimages/" + uniqueFileName;
                }


                library.AdminId = id;
                // Save user to database
                await _connect.Libraries.AddAsync(library);
                await _connect.SaveChangesAsync();
               
                return new { success = true, message = "Registration successful." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new { success = false, message = "An error occurred while adding the user." };
            }
        }

        public async Task<bool> GetAdmindetails(int id)
        {
            bool res = await _connect.Libraries.AnyAsync(x => x.AdminId == id);
            return res;
        }

        public async Task<int?> GetUserIdByEmail(string email)
        {
            return await _connect.Members
                .Where(u => u.Email == email)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync(); // Retrieves the first match or null
        }


        public async Task<string> fetchEmail(string cred)
        {
            return await _connect.Members
                .Where(u => u.Email == cred || u.Name == cred)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }


        public async Task<Member> GetUserDataByEmail(string email)
        {
            return await _connect.Members.FirstOrDefaultAsync(x => x.Email == email);
        }



        public async Task<bool> isemailexitsAsync(string Email)
        {
            return await _connect.Members.AnyAsync(x => x.Email == Email);
        }

        public async Task<bool> ismembernameexitsAsync(string Membername)
        {
            return await _connect.Members.AnyAsync(x => x.Name == Membername);
        }
        public async Task<bool> OtpVerification(string otp)
        {
            return await _connect.Members.AnyAsync(x => x.Otp == otp && x.Otpexpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _connect.Members.FirstOrDefaultAsync(x => x.Email == Email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                await _connect.SaveChangesAsync();
                return new { success = true, message = "Email verified successfully" };
            }
            return new { success = false, message = "Email not found" };
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _connect.Members.ToListAsync();
        }

        public async Task<bool> IsMemberInLibraryAsync(int memberId, int libraryId)
        {
return await _connect.Memberships
        .AnyAsync(m => m.MemberId == memberId && m.LibraryId == libraryId);
        }

        public async Task<int?> GetLibraryIdByMemberAsync(int memberId)
        {

            var membership = await _connect.Memberships
        .Where(m => m.MemberId == memberId)
        .Select(m => m.LibraryId)
        .FirstOrDefaultAsync();

            Console.WriteLine($"LOG: Member {memberId} belongs to Library {membership}");
            return membership;
            //      var membership = await _connect.Memberships
            //.Where(m => m.MemberId == memberId)
            //.OrderByDescending(m => m.MembershipId) // Latest Membership lo
            //.Select(m => m.LibraryId)
            //.FirstOrDefaultAsync();

            //      Console.WriteLine($"Fetched LibraryId for Member {memberId}: {membership}");
            //      return membership;
            //   var membership = await _connect.Memberships
            //.Where(m => m.MemberId == memberId) // ✅ MemberId match karo
            //.Select(m => m.LibraryId) // ✅ Sirf LibraryId lo
            //.FirstOrDefaultAsync();

            //   return membership;// ✅ Agar nahi mila to 0 return hoga
        }



        public async Task<List<Member>> GetLibraryMembersAsync(int libraryId)
        {
            var members = await _connect.Members
        .Include(m => m.Memberships)
        .Where(m => m.Memberships.Any(mem => mem.LibraryId == libraryId))
        .ToListAsync();

            Console.WriteLine($"LOG: Found {members.Count} members for Library {libraryId}");

            return members;
            //return await _connect.Members
            //    .Where(m => _connect.Memberships
            //        .Any(ms => ms.MemberId == m.Id && ms.LibraryId == libraryId)
            //    )
            //    .ToListAsync();
        }


        public async Task<object> AddLibrarianAsync(Member librarian, int adminLibraryId)
        {
            try
            {
                if (librarian.profilefile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    const long maxFileSize = 5 * 1024 * 1024;

                    string fileExtension = Path.GetExtension(librarian.profilefile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return new { success = false, message = "Invalid file type." };
                    }

                    if (librarian.profilefile.Length > maxFileSize)
                    {
                        return new { success = false, message = "File size exceeds 5 MB." };
                    }

                    string uploadFolder = Path.Combine(_environment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(librarian.profilefile.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await librarian.profilefile.CopyToAsync(stream);
                    }
                    librarian.Picture = "/images/" + uniqueFileName;
                }

                // Auto-assign library of admin
                librarian.LibraryId = adminLibraryId;
                librarian.RoleId = 4; // ✅ Librarian RoleId set to 4
                librarian.Password = BCrypt.Net.BCrypt.HashPassword(librarian.Password);

                // ✅ Directly approve librarian (No OTP)
                librarian.Otp = null;
                librarian.Otpexpiry = null;
                librarian.IsEmailVerified = true;
                librarian.VerificationStatus = "Accepted";

                await _connect.Members.AddAsync(librarian);
                await _connect.SaveChangesAsync();

                return new { success = true, message = "Librarian added successfully." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error: " + ex.Message };
            }
        }



        public async Task<Member> GetUserData(int id)
        {
            return await _connect.Members.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<object> UpdateUserDetailsAsync(Member user)
        {
            var existingUser = await _connect.Members.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (existingUser == null)
            {
                return new { success = false, message = "User not found." };
            }

            if (user.profilefile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Maximum allowed file size (5 MB in bytes)
                const long maxFileSize = 5 * 1024 * 1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(user.profilefile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed." };
                }

                // Check file size
                if (user.profilefile.Length > maxFileSize)
                {
                    return new { success = false, message = "File size exceeds the maximum allowed size of 5 MB." };
                }

                string uploadFolder = Path.Combine(_environment.WebRootPath, "images");
                Directory.CreateDirectory(uploadFolder); // Ensure directory exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(user.profilefile.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await user.profilefile.CopyToAsync(stream);
                }
                user.Picture = "/images/" + uniqueFileName;
            }

            existingUser.Name = user.Name;
            existingUser.Gender = user.Gender;
            existingUser.Phoneno = user.Phoneno;
            existingUser.Gender = user.Gender;
            existingUser.State = user.State;
            existingUser.City = user.City;
            existingUser.Address = user.Address;
            existingUser.Email = user.Email;

            if (user.profilefile != null)
            {
                existingUser.Picture = user.Picture;
            }

            await _connect.SaveChangesAsync();

            // Send email notification
            string subject = "Your profile has been updated";
            string body = $"Hello {existingUser.Name},<br><br>Your profile details have been successfully updated.<br><br>Thank you!";
            await _emailSender.SendEmailAsync(existingUser.Email, subject, body);

            return new { success = true, message = "Profile updated successfully!" };
        }
    }




}
//}

