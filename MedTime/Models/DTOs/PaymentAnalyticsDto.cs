using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class PaymentSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int PaidTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int CancelledTransactions { get; set; }
        public decimal AverageRevenuePerPaidTransaction { get; set; }
        public decimal PaidConversionRate { get; set; }
    }

    public class PaymentDailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int PaidTransactions { get; set; }
    }

    public class PaymentPlanBreakdownDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Transactions { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal RevenueShare { get; set; }
    }

    public class PaymentStatusBreakdownDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatusEnum Status { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentTopCustomerDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public decimal TotalSpent { get; set; }
        public int Transactions { get; set; }
        public DateTime? LastPaymentAt { get; set; }
    }

    public class PaymentRecentTransactionDto
    {
        public int PaymentId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatusEnum Status { get; set; }

        public string? TransactionId { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
