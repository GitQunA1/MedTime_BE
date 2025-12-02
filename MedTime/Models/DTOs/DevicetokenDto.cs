using MedTime.Models.Enums;
using System.Text.Json.Serialization;

namespace MedTime.Models.DTOs
{
    public class DevicetokenDto
    {
        public int Tokenid { get; set; }
        public int Userid { get; set; }
        public string Token { get; set; } = null!;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceTypeEnum? DeviceType { get; set; }
        
        public string? DeviceName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
