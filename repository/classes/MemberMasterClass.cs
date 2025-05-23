﻿using library_management.repository.internalinterface;
using library_management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace library_management.Controllers
{
    public class MemberMasterClass : MemberMasterInterface
    {
        private readonly dbConnect _context;
        private readonly EmailSenderInterface _emailSender;
        private readonly BookServiceInterface _bookService;
        public MemberMasterClass(dbConnect context, EmailSenderInterface emailSender, BookServiceInterface bookService)
        {
            _context = context;
            _emailSender = emailSender;
            _bookService = bookService;
        }
        //public async Task<List<Member>> GetAllMembersAsync()
        //{
        //    return await _context.Members.ToListAsync();
        //}
        public async Task<List<MemberListViewModel>> GetAllMembersData(int roleId)
        {
            if (roleId == 1) // Super Admin
            {
                var result = await _context.Members
                    .Include(m => m.Memberships) // Include memberships to get the library information
                    .ThenInclude(ms => ms.Library) // Include the associated library
                    .Select(m => new MemberListViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Email = m.Email,
                        VerificationStatus = m.VerificationStatus,
                        Picture = m.Picture,
                        Libraries = string.Join(", ", m.Memberships.Select(ms => ms.Library.Libraryname)), // Collect library names from the membership
                        RoleId = roleId
                    })
                    .ToListAsync();
                return result;
            }
            else
            {
                var libId = _bookService.GetLoggedInLibrarianLibraryId();
                var result = await _context.Members
                    .Where(m => m.Memberships.Any(ms => ms.LibraryId == libId)) // Filter members by the logged-in librarian's library
                    .Include(m => m.Memberships) // Include memberships
                    .ThenInclude(ms => ms.Library) // Include the library information
                    .Select(m => new MemberListViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Email = m.Email,
                        VerificationStatus = m.VerificationStatus,
                        Picture = m.Picture,
                        Libraries = string.Join(", ", m.Memberships.Where(ms => ms.LibraryId == libId).Select(ms => ms.Library.Libraryname)), // Collect library names specific to the librarian's library
                        RoleId = roleId
                    })
                    .ToListAsync();
                return result;
            }
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

        public async Task<object> AddMember(Member user, int libraryId)
        {
            bool needMembership = false;
            if(user.Id == 0)
            {
                needMembership = true;
            }
            var UpdatedUser = await _context.Members.FirstOrDefaultAsync(u => u.Id == user.Id);
            string imgPath = "";

            if (user.profilefile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
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
                // Update existing
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
            }
            else
            {
                // New user setup
                if (user.Password != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                user.profilefile = null;
                user.Picture = imgPath;
                user.VerificationStatus = "Accepted";
                user.RoleId = 3;

                await _context.Members.AddAsync(user);
            }

            await _context.SaveChangesAsync(); // 🟢 yahan tak user.Id generate ho chuka hoga

            // ✅ Add Membership after user is saved
            if (user.Id > 0 && needMembership)
            {
                var membership = new Membership
                {
                    MemberId = user.Id,
                    LibraryId = libraryId,
                    IsActive = true,
                    IsDeleted = false,
                };
                _context.Memberships.Add(membership);
                await _context.SaveChangesAsync();
            }

            string msg = user.Id > 0 ? "User data has been updated successfully" : "New User has been added successfully";
            return new { success = true, message = msg };
        }



        //public async Task<object> AddMember(Member user)
        //{
        //    var UpdatedUser = await _context.Members.FirstOrDefaultAsync(u => u.Id == user.Id);
        //    string imgPath = "";
        //    if (user.profilefile != null)
        //    {
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        //        // Maximum allowed file size (5 MB in bytes)
        //        //const long maxFileSize = 5  1024  1024;

        //        // Get file extension and check if it is allowed
        //        string fileExtension = Path.GetExtension(user.profilefile.FileName).ToLower();
        //        if (!allowedExtensions.Contains(fileExtension))
        //        {
        //            return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
        //        }

        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images");
        //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + user.profilefile.FileName;

        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            user.profilefile.CopyTo(stream);
        //        }

        //        imgPath = "\\images\\" + uniqueFileName;
        //    }
        //    if (user.Id > 0)
        //    {

        //        UpdatedUser.Name = user.Name;
        //        UpdatedUser.Email = user.Email;
        //        UpdatedUser.Gender = user.Gender;
        //        UpdatedUser.Phoneno = user.Phoneno;
        //        UpdatedUser.Address = user.Address;
        //        UpdatedUser.State = user.State;
        //        UpdatedUser.City = user.City;
        //        UpdatedUser.Joiningdate = user.Joiningdate;
        //        if (user.profilefile != null)
        //        {
        //            UpdatedUser.Picture = imgPath;
        //        }
        //        UpdatedUser.Email = user.Email;
        //    }
        //    if (user.Password != null)
        //    {
        //        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        //    }
        //    user.profilefile = null;
        //    user.Picture = imgPath;    
        //    user.VerificationStatus = "Accepted";
        //    user.RoleId = 3;
        //    string msg = "";
        //    if (user.Id > 0)
        //    {
        //        _context.Update(UpdatedUser);
        //        msg = "User data has been updated successfully";
        //    }
        //    else
        //    {
        //        _context.Members.AddAsync(user);

        //        msg = "New User has been added successfully";
        //    }
        //    await _context.SaveChangesAsync();

        //    return new { success = true, message = msg };
        //}


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
        public async Task<List<MemberListViewModel>> GetMembersByLibraryId(int? libraryId)
        {
            var query = _context.Members
                .Include(m => m.Memberships)
                .ThenInclude(ms => ms.Library)
                .AsQueryable();

            if (libraryId.HasValue)
            {
                query = query.Where(m => m.Memberships.Any(ms => ms.LibraryId == libraryId.Value));
            }

            var result = await query.Select(m => new MemberListViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                VerificationStatus = m.VerificationStatus,
                Picture = m.Picture,
                Libraries = string.Join(", ", m.Memberships.Select(ms => ms.Library.Libraryname)),
                RoleId = 1 // Since only Super Admin is using this
            }).ToListAsync();

            return result;
        }

    }
}

