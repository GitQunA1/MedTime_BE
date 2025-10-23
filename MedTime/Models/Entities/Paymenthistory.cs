using MedTime.Models.Enums;
using System;

namespace MedTime.Models.Entities
{
    public class Paymenthistory
    {
        public int Paymentid { get; set; }

        public int Userid { get; set; }

        public int Planid { get; set; }

        public string Orderid { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentStatusEnum Status { get; set; }

        public string? Payosresponse { get; set; }

        public string? Transactionid { get; set; }

        public DateTime? Paidat { get; set; }

        public DateTime? Createdat { get; set; }

        public DateTime? Updatedat { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Premiumplan Plan { get; set; } = null!;
    }
}
