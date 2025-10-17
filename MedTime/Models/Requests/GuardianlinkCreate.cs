using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class GuardianlinkCreate
    {
        /// <summary>
        /// Uniquecode của người bị theo dõi (Patient)
        /// Frontend nhập code này để tìm user
        /// </summary>
        [Required(ErrorMessage = "Uniquecode is required")]
        [StringLength(50, ErrorMessage = "Uniquecode cannot exceed 50 characters")]
        public string Uniquecode { get; set; } = string.Empty;
    }
}

