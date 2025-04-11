namespace library_management.Models
{
    public class DashboardViewModel
    {
        public int TotalLibraries { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalMembers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBorrows { get; set; }
        public int TotalDistinctBooks { get; set; }
        public int OverdueBooks { get; set; }
        public int PendingAdminApprovals { get; set; }
        public int PendingMemberApprovals { get; set; }

        public string Month { get; set; }
        public int AdminCount { get; set; }
        public int MemberCount { get; set; }
        public int BorrowCount { get; set; }
        public int ReturnCount { get; set; }

        public int? RoleId { get; set; } // <-- Add this property
    }
}
