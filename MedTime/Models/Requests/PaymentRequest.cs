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

    public class PayOSWebhookRequest
    {
        public string Code { get; set; } = null!;
        public string Desc { get; set; } = null!;
        public PayOSWebhookData? Data { get; set; }
        public string Signature { get; set; } = null!;
    }

    public class PayOSWebhookData
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string? Reference { get; set; }  // Transaction ID
        public string TransactionDateTime { get; set; } = null!;
        public string? PaymentLinkId { get; set; }
    }
}
