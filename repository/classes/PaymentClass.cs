using library_management.repository.internalinterface;
using Razorpay.Api;
using library_management.Models;
namespace library_management.repository.classes
{
    public class PaymentClass : PaymentInterface
    {
        private readonly RazorpaySettingClass _razorpaySettings;

        public PaymentClass(RazorpaySettingClass razorpaySettings)
        {
            _razorpaySettings = razorpaySettings;
        }
        public async Task<string> CreatePaymentAsync(int amountInPaise, string currency, string receipt)
        {
            RazorpayClient client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amountInPaise); // Amount in paise (100 INR = 10000 paise)
            options.Add("currency", currency);
            options.Add("receipt", receipt);
            options.Add("payment_capture", 1); // Auto-capture payment

            Order order = client.Order.Create(options);

            return order["id"].ToString(); // Return Razorpay OrderId
        }
    }
}
