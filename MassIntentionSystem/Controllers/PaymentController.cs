using Microsoft.AspNetCore.Mvc;
using MassIntentionSystem.Services;

namespace MassIntentionSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: /Payment/VerifyStatus (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> VerifyStatus([FromBody] PaymentVerificationModel model)
        {
            if (model == null || model.Id == 0)
            {
                return Json(new { success = false, message = "Invalid parameters." });
            }

            bool result = await _paymentService.VerifyPaymentAsync(model.Id, model.Status);

            if (result)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Hindi mahanap ang intention record." });
        }
    }

    public class PaymentVerificationModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty; // "Verified" o "Cancelled"
    }
}