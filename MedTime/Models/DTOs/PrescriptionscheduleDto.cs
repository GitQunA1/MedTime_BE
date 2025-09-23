using MedTime.Models.Enums;

namespace MedTime.Models.DTOs
{
    public class PrescriptionscheduleDto
    {
        public int Scheduleid { get; set; }

        public int Prescriptionid { get; set; }

        public TimeOnly Timeofday { get; set; }

        public int? Interval { get; set; }

        public int? Dayofmonth { get; set; }

        public RepeatPatternEnum RepeatPattern { get; set; } = RepeatPatternEnum.DAILY;
        public DayOfWeekEnum? DayOfWeek { get; set; }

        public bool? Notificationenabled { get; set; }

        public string? Customringtone { get; set; }
    }
}
