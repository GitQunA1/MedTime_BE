using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MedTime.Services
{
    public class AuthService
    {
        private readonly AuthRepo _authRepo;
        private readonly JwtHelper _jwtHelper;
        private readonly IMapper _mapper;
        private readonly TokenCacheService _tokenCache;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            AuthRepo authRepo,
            JwtHelper jwtHelper,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher,
            TokenCacheService tokenCache)
        {
            _authRepo = authRepo;
            _jwtHelper = jwtHelper;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _tokenCache = tokenCache;
        }

        public async Task<UserDto?> RegisterAsync(RegisterRequest request)
        {
            // check username exists
            var existingUser = await _authRepo.GetByUsernameAsync(request.UserName);
            if (existingUser != null)
                return null;

            // Generate unique 6-digit code
            string uniqueCode;
            bool isUnique;
            do
            {
                uniqueCode = GenerateUniqueCode();
                isUnique = await _authRepo.IsUniqueCodeAvailableAsync(uniqueCode);
            } while (!isUnique);

            var newUser = new User
            {
                UserName = request.UserName,
                Passwordhash = _passwordHasher.HashPassword(null!, request.Password),
                Role = Models.Enums.UserRoleEnum.USER,
                Uniquecode = uniqueCode,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var createdUser = await _authRepo.CreateUserAsync(newUser);

            return _mapper.Map<UserDto>(createdUser);
        }

        private string GenerateUniqueCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _authRepo.GetByUsernameAsync(request.UserName);
            if (user == null) return null;

            var passwordVerification = _passwordHasher.VerifyHashedPassword(
                user,
                user.Passwordhash,
                request.Password);

            if (passwordVerification == PasswordVerificationResult.Failed)
                return null;

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // Chỉ lưu vào cache, không lưu vào database
            _tokenCache.StoreRefreshToken(user.Userid, refreshToken, refreshTokenExpiry);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = _jwtHelper.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) return null;

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Kiểm tra refresh token từ cache thay vì database
            var (cachedToken, expiryTime) = _tokenCache.GetRefreshToken(userId);
            if (cachedToken != request.RefreshToken || expiryTime < DateTime.UtcNow)
                return null;

            var user = await _authRepo.GetByUsernameAsync(principal.FindFirst(ClaimTypes.Name)?.Value!);
            if (user == null) return null;

            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();
            var newExpiryTime = DateTime.UtcNow.AddDays(7);

            // Cập nhật token mới vào cache
            _tokenCache.StoreRefreshToken(userId, newRefreshToken, newExpiryTime);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            // Xóa refresh token khỏi cache thay vì database
            _tokenCache.RemoveRefreshToken(userId);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            // Lấy user hiện tại
            var user = await _authRepo.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Verify current password
            var passwordVerification = _passwordHasher.VerifyHashedPassword(
                user,
                user.Passwordhash,
                request.CurrentPassword);

            if (passwordVerification == PasswordVerificationResult.Failed)
                return false;

            // Hash new password
            user.Passwordhash = _passwordHasher.HashPassword(user, request.NewPassword);

            // Update password in database
            var result = await _authRepo.UpdatePasswordAsync(user);

            // Nếu đổi password thành công, xóa tất cả refresh tokens (logout all devices)
            if (result)
            {
                _tokenCache.RemoveRefreshToken(userId);
            }

            return result;
        }
    }
}
