using AutoMapper;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedTime.Services
{
    public class DevicetokenService
    {
        private readonly DevicetokenRepo _devicetokenRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DevicetokenService> _logger;

        public DevicetokenService(
            DevicetokenRepo devicetokenRepo,
            IMapper mapper,
            ILogger<DevicetokenService> logger)
        {
            _devicetokenRepo = devicetokenRepo;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký token mới (hoặc cập nhật nếu đã tồn tại)
        /// </summary>
        public async Task<DevicetokenDto> RegisterTokenAsync(int userId, RegisterDeviceTokenRequest request)
        {
            try
            {
                // Kiểm tra token đã tồn tại chưa
                var existingToken = await _devicetokenRepo.GetByUserIdAndTokenAsync(userId, request.Token);

                if (existingToken != null)
                {
                    // Cập nhật thông tin device
                    existingToken.DeviceType = request.DeviceType;
                    existingToken.DeviceName = request.DeviceName;
                    existingToken.IsActive = true;
                    existingToken.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                    await _devicetokenRepo.UpdateAsync(existingToken.Tokenid, existingToken);
                    _logger.LogInformation($"Updated existing token for User {userId}");

                    return _mapper.Map<DevicetokenDto>(existingToken);
                }

                // Tạo token mới
                var newToken = new Devicetoken
                {
                    Userid = userId,
                    Token = request.Token,
                    DeviceType = request.DeviceType,
                    DeviceName = request.DeviceName,
                    IsActive = true,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                var created = await _devicetokenRepo.CreateAsync(newToken);
                _logger.LogInformation($"Registered new token for User {userId}");

                return _mapper.Map<DevicetokenDto>(created);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, $"Database error registering token for User {userId}");
                
                if (dbEx.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    throw new InvalidOperationException($"User with ID {userId} does not exist in the database.");
                }
                else if (dbEx.InnerException?.Message.Contains("unique constraint") == true)
                {
                    throw new InvalidOperationException("This device token is already registered.");
                }
                
                throw new InvalidOperationException($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering token for User {userId}");
                throw;
            }
        }

        /// <summary>
        /// Hủy đăng ký token (logout 1 thiết bị)
        /// </summary>
        public async Task<bool> UnregisterTokenAsync(string token)
        {
            try
            {
                var deleted = await _devicetokenRepo.DeleteByTokenAsync(token);
                
                if (deleted)
                {
                    _logger.LogInformation($"Unregistered token: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering token");
                throw;
            }
        }

        /// <summary>
        /// Hủy tất cả tokens của user (logout all devices)
        /// </summary>
        public async Task DeactivateAllTokensAsync(int userId)
        {
            try
            {
                await _devicetokenRepo.DeactivateAllUserTokensAsync(userId);
                _logger.LogInformation($"Deactivated all tokens for User {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating tokens for User {userId}");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách tokens active của user
        /// </summary>
        public async Task<List<DevicetokenDto>> GetActiveTokensAsync(int userId)
        {
            var tokens = await _devicetokenRepo.GetActiveTokensByUserIdAsync(userId);
            return _mapper.Map<List<DevicetokenDto>>(tokens);
        }
    }
}
