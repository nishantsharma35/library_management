using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Mvc;

namespace library_management.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentInterface _payment;

        public PaymentController(PaymentInterface payment)
        {
            _payment = payment;
        }

        [HttpPost]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestClass model)
        {
            // model will have amount, order id etc.
            int amountInPaise = (int)(model.FineAmount * 100); // Razorpay needs paise
            string receiptId = "rcpt_" + model.FineId;

            var orderId = await _payment.CreatePaymentAsync(amountInPaise, "INR", receiptId);

            return Ok(new { success = true, orderId = orderId });
        }
    }
}