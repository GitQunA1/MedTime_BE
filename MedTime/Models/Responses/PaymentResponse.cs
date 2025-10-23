namespace MedTime.Models.Responses
{
    public class CreatePaymentResponse
    {
        public string OrderId { get; set; } = null!;
        public string CheckoutUrl { get; set; } = null!;
        public string QrCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PlanName { get; set; } = null!;
    }

    public class PaymentStatusResponse
    {
        public string OrderId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
    }

    public class PayOSWebhookData
    {
        public string OrderCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string Reference { get; set; } = null!;
        public string TransactionDateTime { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string PaymentLinkId { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Desc { get; set; } = null!;
        public string CounterAccountBankId { get; set; } = null!;
        public string CounterAccountBankName { get; set; } = null!;
        public string CounterAccountName { get; set; } = null!;
        public string CounterAccountNumber { get; set; } = null!;
        public string VirtualAccountName { get; set; } = null!;
        public string VirtualAccountNumber { get; set; } = null!;
    }
}
