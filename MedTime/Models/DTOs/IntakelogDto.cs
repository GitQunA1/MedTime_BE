using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class IntakelogDto
    {
        public int Logid { get; set; }

        public int Prescriptionid { get; set; }

        public int Userid { get; set; }

        public int? Scheduleid { get; set; }

        public DateTime Remindertime { get; set; }

        public DateTime? Actiontime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IntakeActionEnum? Action { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConfirmedByEnum? ConfirmedBy { get; set; }


        public string? Notes { get; set; }
    }
}
