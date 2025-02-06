namespace library_management.Models
{
    public class LoginModel
    {
        public string? EmailOrName { get; set; }  
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
