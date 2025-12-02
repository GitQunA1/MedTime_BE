using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class PrescriptionscheduleCreate
    {
        [Required(ErrorMessage = "Prescription ID is required")]
        public int Prescriptionid { get; set; }

        [Required(ErrorMessage = "Time of day is required")]
        public TimeOnly Timeofday { get; set; }

        [Range(1, 365, ErrorMessage = "Interval must be between 1 and 365 days")]
        public int? Interval { get; set; }

        [Range(1, 31, ErrorMessage = "Day of month must be between 1 and 31")]
        public int? Dayofmonth { get; set; }

        [Required(ErrorMessage = "Repeat pattern is required")]
        public RepeatPatternEnum RepeatPattern { get; set; }

        public DayOfWeekEnum? DayOfWeek { get; set; }

        public bool? Notificationenabled { get; set; }

        [StringLength(200, ErrorMessage = "Custom ringtone path cannot exceed 200 characters")]
        public string? Customringtone { get; set; }
    }
}
