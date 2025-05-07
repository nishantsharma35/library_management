namespace library_management.Models
{
    public class MemberListViewModel
    {
        public int Id { get; set; } // Member's unique ID
        public string Name { get; set; } // Member's Name
        public string Email { get; set; } // Member's Email
        public DateTime JoiningDate { get; set; } // Member's Joining Date
        public string VerificationStatus { get; set; } // Status like Pending, Accepted, etc.
        public string Picture { get; set; } // URL to the Profile Picture
        public string Libraries { get; set; } // List of Libraries the Member is associated with (comma-separated)
        public int RoleId { get; set; }
    }

}
