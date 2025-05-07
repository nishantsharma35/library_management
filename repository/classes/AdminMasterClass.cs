using DocumentFormat.OpenXml.Spreadsheet;
using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace library_management.repository.classes
{
    public class AdminMasterClass : AdminInterface
    {
        private readonly dbConnect _connect;
        private readonly EmailSenderInterface _emailSender;

        public AdminMasterClass(dbConnect connect, EmailSenderInterface emailSender)
        {
            _connect = connect;
            _emailSender = emailSender;
        }

        public Library checkExistence(string name, int UserId)
        {
            return _connect.Libraries
                   .FirstOrDefault(l => l.Libraryname == name && l.AdminId != UserId);
        }

        public async Task<List<Library>> GetAllAdminsData()
        {
            return await _connect.Libraries.ToListAsync();
        }

        public async Task<object> UpdateStatus(int mainadminId, bool status)
        {
            try
            {
                var user = await _connect.Members.FirstOrDefaultAsync(u => u.Id == mainadminId);
                if (user == null) return new { success = false, message = "Admin not found." };

                user.VerificationStatus = status ? "Accepted" : "Rejected";
                _connect.Members.Update(user);
                await _connect.SaveChangesAsync();

                var userData = await _connect.Members.FirstOrDefaultAsync(u => u.Id == mainadminId);
                var subject = status ? "Account Accepted" : "Account Rejected";
                var body = status
                    ? $"Hi {user.Name},<br>Your Admin account is accepted. Log in to start."
                    : $"Hi {user.Name},<br>Your Admin account is rejected. Please contact support.";
                await _emailSender.SendEmailAsync(user.Email, subject, body);

                return new { success = true, message = status ? "Admin accepted." : "Admin rejected." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }


        public async Task<object> AddAdmin(Library admin)
        {
            // ✅ Fetch existing library if updating
            var UpdatedUser = await _connect.Libraries.FirstOrDefaultAsync(u => u.LibraryId == admin.LibraryId);
            string imgPath = "";

            // ✅ Handling Image Upload if provided
            if (admin.libraryfile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // ✅ Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(admin.libraryfile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\libraryimages");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + admin.libraryfile.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await admin.libraryfile.CopyToAsync(stream);
                }

                imgPath = "\\libraryimages\\" + uniqueFileName;
            }

            // ✅ Update or Add Library/Admin data based on LibraryId
            if (UpdatedUser != null)  // 🛠 FIXED: Ensure record exists before updating
            {
                Console.WriteLine("Updating Admin with ID: " + admin.LibraryId);  // Debugging Line

                UpdatedUser.Libraryname = admin.Libraryname;
                UpdatedUser.AdminId = admin.AdminId;
                UpdatedUser.State = admin.State;
                UpdatedUser.City = admin.City;
                UpdatedUser.Address = admin.Address;
                UpdatedUser.Pincode = admin.Pincode;
                UpdatedUser.StartTime = admin.StartTime;
                UpdatedUser.ClosingTime = admin.ClosingTime;

                if (admin.libraryfile != null)
                {
                    UpdatedUser.LibraryImagePath = imgPath;
                }

                _connect.Update(UpdatedUser);
                await _connect.SaveChangesAsync();
                return new { success = true, message = "Admin data has been updated successfully" };
            }
            else
            {
                Console.WriteLine("Adding New Admin...");  // Debugging Line
                _connect.Add(admin);
                await _connect.SaveChangesAsync();
                return new { success = true, message = "New Admin has been added successfully" };
            }
        }



    }
}

















//using library_management.Models;
//using library_management.repository.internalinterface;
//using Microsoft.EntityFrameworkCore;

//namespace library_management.repository.classes
//{
//    public class AdminMasterClass : AdminInterface
//     {
//        private readonly dbConnect _connect;
//        private readonly EmailSenderInterface _emailSender;


//        public AdminMasterClass(dbConnect connect, EmailSenderInterface emailSender)
//        {
//            _connect = connect;
//            _emailSender = emailSender;
//        }

//        public async Task<List<Library>> GetAllAdminsData()
//        {
//            return await _connect.Libraries.ToListAsync();
//        }

//        public async Task<object> UpdateStatus(int mainadminId, bool status)
//        {
//            try
//            {
//                var user = await _connect.Members.FirstOrDefaultAsync(u => u.Id == mainadminId);
//                if (user == null) return new { success = false, message = "Admin not found." };

//                user.VerificationStatus = status ? "Accepted" : "Rejected";
//                _connect.Members.Update(user);
//                await _connect.SaveChangesAsync();

//                var userData = await _connect.Members.FirstOrDefaultAsync(u => u.Id == mainadminId);
//                var subject = status ? "Account Accepted" : "Account Rejected";
//                var body = status
//                    ? $"Hi {user.Name},<br>Your Admin account is accepted. Log in to start."
//                    : $"Hi {user.Name},<br>Your Admin account is rejected. Please contact support.";
//                await _emailSender.SendEmailAsync(user.Email, subject, body);

//                return new { success = true, message = status ? "Admin accepted." : "Admin rejected." };
//            }
//            catch (Exception ex)
//            {
//                return new { success = false, message = ex.Message };
//            }

//        }

//            public async Task<object> AddAdmin(Library admin)

//        {
//            var UpdatedUser = await _connect.Libraries.FirstOrDefaultAsync(u => u.LibraryId == admin.LibraryId);
//            string imgPath = "";
//            if (admin.libraryfile != null)
//            {
//                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

//                //Maximum allowed file size(5 MB in bytes)
//                //const long maxFileSize = 5  1024  1024;

//                // Get file extension and check if it is allowed
//                string fileExtension = Path.GetExtension(admin.libraryfile.FileName).ToLower();
//                if (!allowedExtensions.Contains(fileExtension))
//                {
//                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
//                }

//                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\libraryimages");
//                string uniqueFileName = Guid.NewGuid().ToString() + "_" + admin.libraryfile.FileName;

//                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    admin.libraryfile.CopyTo(stream);
//                }

//                imgPath = "\\libraryimages\\" + uniqueFileName;
//            }
//            if (admin.LibraryId > 0)
//            {

//                UpdatedUser.Libraryname = admin.Libraryname;
//                UpdatedUser.AdminId = admin.AdminId;
//            //      UpdatedUser.CreatedAt = admin.CreatedAt;
//                UpdatedUser.State = admin.State;
//                UpdatedUser.City = admin.City;
//                UpdatedUser.Address = admin.Address;
//                UpdatedUser.Pincode = admin.Pincode;
//                UpdatedUser.StartTime = admin.StartTime;
//                UpdatedUser.ClosingTime = admin.ClosingTime;
//                if (admin.libraryfile != null)
//                {
//                    UpdatedUser.LibraryImagePath = imgPath;
//                }

//            }
//            string msg = "";

//            if (admin.AdminId > 0)
//            {
//                _connect.Update(UpdatedUser);
//                msg = "Admin data has been updated successfully";
//            }
//            else
//            {
//                _connect.AddAsync(admin);
//                msg = "New Admin has been added successfully";
//            }
//            await _connect.SaveChangesAsync();


//            return new { success = true, message = msg };
//        }

//        public Library checkExistence(string Username, int UserId)
//        {
//            var existingUser = _connect.Libraries
//            .Where(u => u.LibraryId != UserId) // Exclude the current admin being edited
//            .FirstOrDefault(u => u.Libraryname == Username);

//            return existingUser;
//        }

//    }
//}
