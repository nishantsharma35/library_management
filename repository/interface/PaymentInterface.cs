namespace library_management.repository.internalinterface
{
    public interface PaymentInterface
    {
        Task<string> CreatePaymentAsync(int amountInPaise, string currency, string receipt);
    }
}
