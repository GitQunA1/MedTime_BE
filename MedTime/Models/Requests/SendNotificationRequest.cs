using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    /// <summary>
    /// Request để gửi notification ngay lập tức
    /// </summary>
    public class SendNotificationRequest
    {
        /// <summary>
        /// Gửi cho user nào (optional, nếu không có thì gửi cho chính mình)
        /// </summary>
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; } = null!;

        public int? PrescriptionId { get; set; }
        public int? ScheduleId { get; set; }
    }
}
