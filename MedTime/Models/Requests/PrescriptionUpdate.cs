using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class PrescriptionUpdate
    {
        [StringLength(100, ErrorMessage = "Dosage cannot exceed 100 characters")]
        public string? Dosage { get; set; }

        [Range(1, 10, ErrorMessage = "Frequency per day must be between 1 and 10")]
        public int? Frequencyperday { get; set; }

        public DateOnly? Startdate { get; set; }

        public DateOnly? Enddate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Remaining quantity must be positive")]
        public int? Remainingquantity { get; set; }

        [StringLength(200, ErrorMessage = "Doctor name cannot exceed 200 characters")]
        public string? Doctorname { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
}
