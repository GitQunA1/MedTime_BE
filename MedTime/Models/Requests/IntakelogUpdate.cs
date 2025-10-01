using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class IntakelogUpdate
    {
        public int? Scheduleid { get; set; }

        [Required(ErrorMessage = "Reminder time is required")]
        public DateTime Remindertime { get; set; }

        public DateTime? Actiontime { get; set; }

        public IntakeActionEnum? Action { get; set; }

        public ConfirmedByEnum? ConfirmedBy { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
