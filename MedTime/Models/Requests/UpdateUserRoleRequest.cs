using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "Role is required")]
        public UserRoleEnum Role { get; set; }
    }
}
