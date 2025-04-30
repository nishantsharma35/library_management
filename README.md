📚 PaperByte - Library Management System (LMS)

PaperByte is a multi-library, web-based Library Management System developed as part of a final year software engineering internship project. The platform enables centralized and decentralized control of library resources across multiple branches, ensuring smooth operations for administrators, librarians, and members.

---

⚙️ Tech Stack

Frontend: HTML, CSS, JavaScript, Bootstrap  
Backend: ASP.NET Core MVC (C#), Dependency Injection  
Database: Microsoft SQL Server  
Authentication: Session-based login, Role-based access  

Libraries / Tools Used:
- Entity Framework Core (ORM)
- MailKit (Email sending)
- ClosedXML (Excel exports)
- Bcrypt (Password hashing)
- DataTables (Table filtering/searching)
- SweetAlert & Toastify (User notifications)

---

🔑 Key Features

👥 User Management
- Member and Admin login with secure password hashing (BCrypt)
- Super Admin panel for managing Admins
- Role-based access control: Super Admin, Library Admin, Member


🏛️ Multi-Library Management
- Multiple libraries support
- Admins linked to specific libraries via membership
- Super Admin monitors all libraries and controls fine settings

📘 Book Management
- Add/edit/delete books with genre and author categorization
- Auto-stock management via LibraryBooks table
- Shared `Books` table with per-library inventory via `LibraryBooks`
- Import/export support for bulk book data

📖 Borrow & Return System
- Member borrowing with due date calculation
- Per-library stock update logic
- Borrow history with status tracking (returned/overdue)
- Fine calculation on late returns using dynamic fine rates

💰 Fine Management
- Fine table with borrow linkage
- Fine amount set per library by Admin or globally by Super Admin
- Payable and paid tracking with payment status control

🛡️ Approval System
- Super Admin approves/rejects Admins
- Admins approve/reject Members joining their library
- Only one library membership allowed per member

📊 Reports & Analytics
- Borrowed books, fine reports, member activity
- Excel export support via ClosedXML
- Role-based access to relevant reports

📬 Notification System
- Email notifications for key actions (planned)
- In-app alerts using Toastify and SweetAlert

🔐 Security & Session Management
- Role-based redirection & session validation
- Member session auto-handling with restriction on multi-library join
- IsDeleted / IsActive flags for soft-deletion

---

🗂️ Project Structure
PaperByte/ 
├── wwwroot/
├── controllers/
├── models/
├── dto/
├── repositories/
├── services/
├── views/
└── utils/
├── config/
├── appsettings.json


---

🚧 Planned Enhancements

- 📬 Email OTP login for passwordless authentication
- 🛒 Book wishlist & reservation feature
- 📈 Dashboard visual analytics with graphs
- 💬 In-app feedback/suggestions panel
- 📲 Facebook OAuth for registration

---

📚 References

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core)
- [Entity Framework Core](https://learn.microsoft.com/ef/core)
- [ClosedXML (Excel export)](https://github.com/ClosedXML/ClosedXML)
- [MailKit](https://github.com/jstedfast/MailKit)
- [SweetAlert2](https://sweetalert2.github.io/)
- [SB Admin 2 Bootstrap Template](https://startbootstrap.com/theme/sb-admin-2)

---

👨‍💻 About the Developer

I'm Nishant Sharma, a BSc IT student and software development intern with a strong interest in building practical, real-world applications. I enjoy designing structured systems that improve efficiency, usability, and scalability.

> 🚀 Always building, always learning.  
> 💬 Let's connect!

---



