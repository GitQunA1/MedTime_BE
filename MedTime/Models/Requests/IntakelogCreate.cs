using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class IntakelogCreate
    {
        [Required(ErrorMessage = "Prescription ID is required")]
        public int Prescriptionid { get; set; }

        /// <summary>
        /// Optional: Nếu có thì lấy TimeOfDay từ PrescriptionSchedule
        /// Nếu null thì backend tính toán từ FrequencyPerDay
        /// </summary>
        public int? Scheduleid { get; set; }

        /// <summary>
        /// Optional: Thời điểm user thực sự uống thuốc (khi confirm)
        /// </summary>
        public DateTime? Actiontime { get; set; }

        /// <summary>
        /// Action user đã thực hiện: TAKEN, MISSED, POSTPONED
        /// </summary>
        public IntakeActionEnum? Action { get; set; }

        /// <summary>
        /// Ai confirm: USER, GUARDIAN, SYSTEM
        /// </summary>
        public ConfirmedByEnum? ConfirmedBy { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
