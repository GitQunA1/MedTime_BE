using MedTime.Models.Enums;

namespace MedTime.Models.DTOs
{
    public class UserDto
    {
        public int Userid { get; set; }

        public string Fullname { get; set; } = null!;

        public DateOnly? Dateofbirth { get; set; }

        public string? Gender { get; set; }

        public string? Phonenumber { get; set; }

        public string Email { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Passwordhash { get; set; } = null!;

        public UserRoleEnum Role { get; set; } = UserRoleEnum.USER;

        public string? Uniquecode { get; set; }

        public string? Timezone { get; set; }

        public bool? Ispremium { get; set; }

        public DateTime? Premiumstart { get; set; }

        public DateTime? Premiumend { get; set; }

        public DateTime? Createdat { get; set; }

        public DateTime? Updatedat { get; set; }
    }
}
