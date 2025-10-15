using MedTime.Helpers;
using MedTime.Models.Requests;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedTime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly DevicetokenService _devicetokenService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            NotificationService notificationService,
            DevicetokenService devicetokenService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _devicetokenService = devicetokenService;
            _logger = logger;
        }

        [HttpPost("device-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                var result = await _devicetokenService.RegisterTokenAsync(userId, request);
                _logger.LogInformation($"User {userId} registered device token successfully");

                return Ok(ApiResponse<object>.SuccessResponse(result, "Device token registered successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to register device token: {ex.Message}", "Bad Request"));
            }
        }

        [HttpDelete("device-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnregisterDeviceToken([FromBody] UnregisterDeviceTokenRequest request)
        {
            try
            {
                var result = await _devicetokenService.UnregisterTokenAsync(request.Token);

                if (result)
                {
                    _logger.LogInformation($"Device token unregistered successfully");
                    return Ok(ApiResponse<object>.SuccessResponse(null!, "Device token unregistered successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token not found or already removed", "Bad Request"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device token");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to unregister device token: {ex.Message}", "Bad Request"));
            }
        }

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value?.ToUpper();

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int tokenUserId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                if (!request.UserId.HasValue)
                {
                    request.UserId = tokenUserId;
                }

                if (userRole != "ADMIN" && request.UserId != tokenUserId)
                {
                    _logger.LogWarning($"User {tokenUserId} attempted to send notification to User {request.UserId}");
                    return Forbid();
                }

                var result = await _notificationService.SendToUserAsync(request);

                return Ok(ApiResponse<object>.SuccessResponse(result, "Notification sent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to send notification: {ex.Message}", "Bad Request"));
            }
        }

        [HttpGet("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetNotificationHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value?.ToUpper();

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int tokenUserId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                int targetUserId = userId ?? tokenUserId;

                if (userRole != "ADMIN" && targetUserId != tokenUserId)
                {
                    _logger.LogWarning($"User {tokenUserId} attempted to access notification history of User {targetUserId}");
                    return Forbid();
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var result = await _notificationService.GetHistoryAsync(targetUserId, pageNumber, pageSize);

                return Ok(ApiResponse<object>.SuccessResponse(result, "Notification history retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification history");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to get notification history: {ex.Message}", "Bad Request"));
            }
        }

        [HttpGet("unread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    count = notifications.Count,
                    notifications = notifications
                }, "Unread notifications retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to get unread notifications: {ex.Message}", "Bad Request"));
            }
        }

        [HttpPut("{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                var result = await _notificationService.MarkAsReadAsync(id);

                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null!, "Notification marked as read"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Notification not found", "Not Found"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to mark notification as read: {ex.Message}", "Bad Request"));
            }
        }

        [HttpPut("mark-all-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok(ApiResponse<object>.SuccessResponse(null!, "All notifications marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to mark all notifications as read: {ex.Message}", "Bad Request"));
            }
        }

        [HttpGet("unread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token", "Unauthorized"));
                }

                var count = await _notificationService.CountUnreadAsync(userId);

                return Ok(ApiResponse<object>.SuccessResponse(new { unreadCount = count }, "Unread count retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to get unread count: {ex.Message}", "Bad Request"));
            }
        }
    }
}
