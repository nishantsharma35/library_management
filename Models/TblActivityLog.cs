using System.ComponentModel.DataAnnotations.Schema;

namespace library_management.Models
{
    public class TblActivityLog
    {
        public int LogId { get; set; }

        public int UserId { get; set; }

        public string? ActivityType { get; set; }

        public string? Description { get; set; }
        [NotMapped]
        public string? ImagePath { get; set; }

        public DateTime? Timestamp { get; set; }

    }
}
