using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class NotificationhistoryDto
    {
        public int Notificationid { get; set; }
        public int Userid { get; set; }
        public int? Prescriptionid { get; set; }
        public int? Scheduleid { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SentAt { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.PENDING;
        
        public string? ErrorMessage { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
