using library_management.Models;
using library_management.repository.internalinterface;

namespace library_management.repository.classes
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly dbConnect _context;

        public ActivityRepository(dbConnect context)
        {
            _context = context;
        }
        public object AddNewActivity(int id, string type, string desc)
        {
            TblActivityLog activity = new TblActivityLog()
            {
                UserId = id,
                ActivityType = type,
                Description = desc
            };
            _context.TblActivitylogs.Add(activity);
            _context.SaveChanges();

            return new { success = true, message = "Activity logged successfully" };
        }
    }
}
