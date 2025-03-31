namespace library_management.repository.internalinterface
{
    public interface EmailSenderInterface
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        string GenerateOtp();

        Task SendEmailWithAttachment(string to, string subject, string body, byte[] attachment, string attachmentName);
    }
}
