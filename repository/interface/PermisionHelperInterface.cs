namespace library_management.repository.internalinterface
{
    public interface PermisionHelperInterface
    {
        public string HasAccess(string tabName, int roleId);
    }
}
