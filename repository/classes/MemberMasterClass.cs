using library_management.repository.internalinterface;
using library_management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace library_management.Controllers
{
    public class MemberMasterClass : MemberMasterInterface
    {
        private readonly dbConnect _context;
        private readonly EmailSenderInterface _emailSender;
        public MemberMasterClass(dbConnect context, EmailSenderInterface emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<object> UpdateStatus(int userId, bool status)
        {
            try
            {
                var user = await _context.Members.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return new { success = false, message = "User not found." };

                user.VerificationStatus = status ? "Accepted" : "Rejected";
                _context.Members.Update(user);
                await _context.SaveChangesAsync();

                var userData = await _context.Members.FirstOrDefaultAsync(u => u.Id == userId);
                var subject = status ? "Account Accepted" : "Account Rejected";
                var body = status
                    ? $"Hi {user.Name},<br>Your account is accepted. Log in to start."
                    : $"Hi {user.Name},<br>Your account is rejected. Please contact support.";
                await _emailSender.SendEmailAsync(user.Email, subject, body);

                return new { success = true, message = status ? "User accepted." : "User rejected." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }
        public async Task<object> AddMember(Member user)
        {
            var UpdatedUser = await _context.Members.FirstOrDefaultAsync(u => u.Id == user.Id);
            string imgPath = "";
            if (user.profilefile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(user.profilefile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + user.profilefile.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    user.profilefile.CopyTo(stream);
                }

                imgPath = "\\images\\" + uniqueFileName;
            }
            if (user.Id > 0)
            {

                UpdatedUser.Name = user.Name;
                UpdatedUser.Email = user.Email;
                UpdatedUser.Gender = user.Gender;
                UpdatedUser.Phoneno = user.Phoneno;
                UpdatedUser.Address = user.Address;
                UpdatedUser.State = user.State;
                UpdatedUser.City = user.City;
                UpdatedUser.Joiningdate = user.Joiningdate;
                if (user.profilefile != null)
                {
                    UpdatedUser.Picture = imgPath;
                }
                UpdatedUser.Email = user.Email;
            }
            if (user.Password != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            user.profilefile = null;
            user.Picture = imgPath;    
            user.VerificationStatus = "Accepted";
            string msg = "";
            if (user.Id > 0)
            {
                _context.Update(UpdatedUser);
                msg = "User data has been updated successfully";
            }
            else
            {
                _context.AddAsync(user);
                msg = "New User has been added successfully";
            }
            await _context.SaveChangesAsync();

            return new { success = true, message = msg };
        }


        public Member checkExistence(string Username, string email, int UserId)
        {
            var existingUser = _context.Members
            .Where(u => u.Id != UserId) // Exclude the current admin being edited
            .FirstOrDefault(u => u.Email == email || u.Name == Username);

            return existingUser;
        }

        public async Task<List<Member>> GetLibraryMembersAsync(int libraryId)
        {
            return await _context.Members
       .Where(m => _context.Memberships.Any(ms => ms.MemberId == m.Id && ms.LibraryId == libraryId)) // ✅ Membership table se check karo
       .ToListAsync();

            //   return await _context.Members
            //.Join(_context.Memberships,
            //      m => m.Id,
            //      ms => ms.MemberId,
            //      (m, ms) => new { Member = m, Membership = ms })
            //.Where(x => x.Membership.LibraryId == libraryId && x.Membership.IsActive) // ✅ Sirf Active Members
            //.Select(x => x.Member)
            //.ToListAsync();
        }

    }
}

