using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace library_management.Models
{
    public class TblTransaction
    {
        public int TransactionId { get; set; }

        public int FineId { get; set; }

        public int AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string PaymentMode { get; set; }

        public string Reference { get; set; }

        // ✅ Navigation Property
        public Fine Fine { get; set; }

    }
}
