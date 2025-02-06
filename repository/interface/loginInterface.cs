namespace library_management.repository.internalinterface
{
    public interface loginInterface
    {
        Task<object> AuthenticateUser(string EmailOrMemberName, string password);
        Task<object> TokenSenderViaEmail(string email);
        Task<object> ResetPassword(string creds, string newPassword);
    }
}
