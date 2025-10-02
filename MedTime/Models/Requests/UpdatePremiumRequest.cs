using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class UpdatePremiumRequest
    {
        [Required(ErrorMessage = "IsPremium status is required")]
        public bool IsPremium { get; set; }

        /// <summary>
        /// Ngày bắt đầu Premium (nếu IsPremium = true)
        /// </summary>
        public DateTime? PremiumStart { get; set; }

        /// <summary>
        /// Ngày kết thúc Premium (nếu IsPremium = true)
        /// </summary>
        public DateTime? PremiumEnd { get; set; }
    }
}
