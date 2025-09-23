using MedTime.Models.Enums;

namespace MedTime.Models.DTOs
{
    public class MedicineDto
    {
        public int Medicineid { get; set; }

        public string Name { get; set; } = null!;

        public decimal? Strengthvalue { get; set; }

        public MedicineTypeEnum? Type { get; set; }
        public MedicineUnitEnum? StrengthUnit { get; set; }

        public string? Imageurl { get; set; }

        public string? Notes { get; set; }
    }
}
