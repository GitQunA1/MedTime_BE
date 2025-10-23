using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class PremiumplanDto
    {
        public int Planid { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PremiumPlanTypeEnum Plantype { get; set; }

        public string Planname { get; set; } = null!;

        public string? Description { get; set; }

        public int Durationdays { get; set; }

        public decimal Price { get; set; }

        public decimal? Discountpercent { get; set; }

        public decimal? Finalprice { get; set; } // Giá sau khi giảm

        public bool Isactive { get; set; }
    }

    public class PaymenthistoryDto
    {
        public int Paymentid { get; set; }

        public int Userid { get; set; }

        public int Planid { get; set; }

        public string Orderid { get; set; } = null!;

        public decimal Amount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatusEnum Status { get; set; }

        public string? Transactionid { get; set; }

        public DateTime? Paidat { get; set; }

        public DateTime? Createdat { get; set; }

        // Navigation properties
        public PremiumplanDto? Plan { get; set; }
    }
}
