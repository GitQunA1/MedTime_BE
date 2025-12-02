using MedTime.Models.DTOs;

namespace MedTime.Models.Responses
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }
}
