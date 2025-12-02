using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;
using Microsoft.Extensions.Logging;

namespace MedTime.Services
{
    public class NotificationService
    {
        private readonly NotificationhistoryRepo _notificationRepo;
        private readonly DevicetokenRepo _devicetokenRepo;
        private readonly FirebaseService _firebaseService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationhistoryRepo notificationRepo,
            DevicetokenRepo devicetokenRepo,
            FirebaseService firebaseService,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _devicetokenRepo = devicetokenRepo;
            _firebaseService = firebaseService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gửi notification đến user ngay lập tức
        /// </summary>
        public async Task<NotificationhistoryDto> SendToUserAsync(SendNotificationRequest request)
        {
            try
            {
                if (!request.UserId.HasValue)
                {
                    throw new ArgumentException("UserId is required");
                }

                // Lấy tất cả active tokens của user
                var deviceTokens = await _devicetokenRepo.GetActiveTokensByUserIdAsync(request.UserId.Value);

                if (deviceTokens.Count == 0)
                {
                    _logger.LogWarning($"No active tokens found for User {request.UserId}");
                    
                    // Lưu history với status FAILED
                    return await SaveHistoryAsync(
                        request.UserId.Value,
                        request.Title,
                        request.Message,
                        NotificationStatusEnum.FAILED,
                        "No active device tokens",
                        request.PrescriptionId,
                        request.ScheduleId
                    );
                }

                // Chuẩn bị data để gửi kèm notification
                var data = new Dictionary<string, string>
                {
                    { "notificationType", "medication_reminder" }
                };

                if (request.PrescriptionId.HasValue)
                    data["prescriptionId"] = request.PrescriptionId.Value.ToString();
                
                if (request.ScheduleId.HasValue)
                    data["scheduleId"] = request.ScheduleId.Value.ToString();

                // Gửi đến tất cả devices của user
                var tokens = deviceTokens.Select(dt => dt.Token).ToList();
                NotificationStatusEnum status;
                string? errorMessage = null;

                try
                {
                    if (tokens.Count == 1)
                    {
                        var messageId = await _firebaseService.SendNotificationAsync(
                            tokens[0], 
                            request.Title, 
                            request.Message, 
                            data
                        );
                        status = NotificationStatusEnum.SENT;
                        _logger.LogInformation($"Notification sent to User {request.UserId}, Firebase ID: {messageId}");
                    }
                    else
                    {
                        var batchResponse = await _firebaseService.SendMulticastNotificationAsync(
                            tokens, 
                            request.Title, 
                            request.Message, 
                            data
                        );
                        
                        status = batchResponse.SuccessCount > 0 
                            ? NotificationStatusEnum.SENT 
                            : NotificationStatusEnum.FAILED;
                        
                        _logger.LogInformation($"Multicast notification sent to User {request.UserId}, Success: {batchResponse.SuccessCount}/{tokens.Count}");
                        
                        if (batchResponse.FailureCount > 0)
                        {
                            errorMessage = $"Failed to send to {batchResponse.FailureCount} device(s)";
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = NotificationStatusEnum.FAILED;
                    errorMessage = ex.Message;
                    _logger.LogError(ex, $"Failed to send notification to User {request.UserId}");
                }

                // Lưu history
                return await SaveHistoryAsync(
                    request.UserId.Value,
                    request.Title,
                    request.Message,
                    status,
                    errorMessage,
                    request.PrescriptionId,
                    request.ScheduleId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendToUserAsync");
                throw;
            }
        }

        /// <summary>
        /// Lưu notification history
        /// </summary>
        public async Task<NotificationhistoryDto> SaveHistoryAsync(
            int userId,
            string title,
            string message,
            NotificationStatusEnum status,
            string? errorMessage = null,
            int? prescriptionId = null,
            int? scheduleId = null)
        {
            var history = new Notificationhistory
            {
                Userid = userId,
                Prescriptionid = prescriptionId,
                Scheduleid = scheduleId,
                Title = title,
                Message = message,
                SentAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Status = status,
                ErrorMessage = errorMessage,
                IsRead = false
            };

            var created = await _notificationRepo.CreateAsync(history);
            return _mapper.Map<NotificationhistoryDto>(created);
        }

        /// <summary>
        /// Lấy notification history có phân trang
        /// </summary>
        public async Task<PaginatedResult<NotificationhistoryDto>> GetHistoryAsync(
            int userId, 
            int pageNumber, 
            int pageSize)
        {
            var query = _notificationRepo.GetByUserIdQuery(userId);
            var paginated = await query.ToPaginatedListAsync(pageNumber, pageSize);

            var mappedItems = _mapper.Map<List<NotificationhistoryDto>>(paginated.Items);
            return new PaginatedResult<NotificationhistoryDto>(
                mappedItems,
                paginated.TotalCount,
                paginated.PageNumber,
                paginated.PageSize
            );
        }

        /// <summary>
        /// Lấy danh sách notifications chưa đọc
        /// </summary>
        public async Task<List<NotificationhistoryDto>> GetUnreadNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepo.GetUnreadByUserIdAsync(userId);
            return _mapper.Map<List<NotificationhistoryDto>>(notifications);
        }

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            return await _notificationRepo.MarkAsReadAsync(notificationId);
        }

        /// <summary>
        /// Đánh dấu tất cả notifications đã đọc
        /// </summary>
        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);
        }

        /// <summary>
        /// Đếm số notifications chưa đọc
        /// </summary>
        public async Task<int> CountUnreadAsync(int userId)
        {
            return await _notificationRepo.CountUnreadAsync(userId);
        }
    }
}
