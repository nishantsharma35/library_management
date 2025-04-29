namespace library_management.Models
{
    public class ActivityLogViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string? ActivityType { get; set; }
        public string? Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? ImagePath { get; set; }

        // Extra info for display
        public string? MemberName { get; set; }
        public string? LibraryName { get; set; }
    }
}
