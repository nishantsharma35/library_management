namespace library_management.Models
{
    public class GoogleSignupModel
    {
        public string Email { get; set; }

        public string Username { get; set; }
        public int RoleId { get; set; }

        // These only required when RoleId == 2 (Library Admin)
       
    }
}
