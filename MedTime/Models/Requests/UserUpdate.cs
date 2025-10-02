using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class UserUpdate
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? Fullname { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phonenumber { get; set; }

        public DateOnly? Dateofbirth { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        [StringLength(50, ErrorMessage = "Timezone cannot exceed 50 characters")]
        public string? Timezone { get; set; }
    }
}
