using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    /// <summary>
    /// Request để đăng ký FCM device token
    /// </summary>
    public class RegisterDeviceTokenRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = null!;

        /// <summary>
        /// ANDROID, IOS, WEB
        /// </summary>
        public DeviceTypeEnum? DeviceType { get; set; }

        public string? DeviceName { get; set; }
    }
}
