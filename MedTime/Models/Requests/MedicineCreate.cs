using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class MedicineCreate
    {
        [Required(ErrorMessage = "Medicine name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Strength value must be positive")]
        public decimal? Strengthvalue { get; set; }

        public MedicineTypeEnum? Type { get; set; }

        public MedicineUnitEnum? StrengthUnit { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? Imageurl { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
}
