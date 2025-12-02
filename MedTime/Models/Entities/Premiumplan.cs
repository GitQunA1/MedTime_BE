using MedTime.Models.Enums;
using System;

namespace MedTime.Models.Entities
{
    public class Premiumplan
    {
        public int Planid { get; set; }

        public PremiumPlanTypeEnum Plantype { get; set; }

        public string Planname { get; set; } = null!;

        public string? Description { get; set; }

        public int Durationdays { get; set; }

        public decimal Price { get; set; }

        public decimal? Discountpercent { get; set; }

        public bool Isactive { get; set; } = true;

        public DateTime? Createdat { get; set; }

        public DateTime? Updatedat { get; set; }

        public virtual ICollection<Paymenthistory> Paymenthistories { get; set; } = new List<Paymenthistory>();
    }
}
