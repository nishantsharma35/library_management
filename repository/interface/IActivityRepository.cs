namespace library_management.repository.internalinterface
{
    public interface IActivityRepository
    {
        object AddNewActivity(int id, string type, string desc);
    }
}
