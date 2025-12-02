using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    /// <summary>
    /// Request để hủy đăng ký device token (logout khỏi thiết bị)
    /// </summary>
    public class UnregisterDeviceTokenRequest
    {
        /// <summary>
        /// Device token cần xóa (FCM registration token)
        /// </summary>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
