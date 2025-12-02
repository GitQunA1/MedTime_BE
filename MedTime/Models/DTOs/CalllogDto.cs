using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class CalllogDto
    {
        public int Callid { get; set; }

        public int Userid { get; set; }

        public int? Scheduleid { get; set; }

        public DateTime? Calltime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CallStatusEnum? Status { get; set; }
    }
}
