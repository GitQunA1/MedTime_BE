using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class EmergencycontactCreate
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phonenumber { get; set; }

        [StringLength(50, ErrorMessage = "Relation cannot exceed 50 characters")]
        public string? Relation { get; set; }
    }
}
