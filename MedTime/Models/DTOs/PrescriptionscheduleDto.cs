using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class PrescriptionscheduleDto
    {
        public int Scheduleid { get; set; }

        public int Prescriptionid { get; set; }

        public TimeOnly Timeofday { get; set; }

        public int? Interval { get; set; }

        public int? Dayofmonth { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RepeatPatternEnum RepeatPattern { get; set; } = RepeatPatternEnum.DAILY;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeekEnum? DayOfWeek { get; set; }

        public bool? Notificationenabled { get; set; }

        public string? Customringtone { get; set; }
    }
}
