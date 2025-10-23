using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class CreatePaymentRequest
    {
        [Required(ErrorMessage = "PlanId is required")]
        public int PlanId { get; set; }

        public string? ReturnUrl { get; set; }

        public string? CancelUrl { get; set; }
    }

    public class PaymentCallbackRequest
    {
        public string Code { get; set; } = null!;
        public string Id { get; set; } = null!;
        public bool Cancel { get; set; }
        public string Status { get; set; } = null!;
        public long OrderCode { get; set; }
    }

    public class VerifyWebhookRequest
    {
        public string Signature { get; set; } = null!;
        public object Data { get; set; } = null!;
    }
}
