using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class MedicineDto
    {
        public int Medicineid { get; set; }

        public string Name { get; set; } = null!;

        public decimal? Strengthvalue { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MedicineTypeEnum? Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MedicineUnitEnum? StrengthUnit { get; set; }

        public string? Imageurl { get; set; }

        public string? Notes { get; set; }
    }
}
