namespace library_management.Models
{
    public class FinePaymentRequest
    {
        public int FineId { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }
        public string TransactionType { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }

    }
}
