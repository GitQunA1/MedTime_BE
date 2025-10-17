using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Requests;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedTime.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public StatisticsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// GET /api/statistics/dashboard
        /// Lấy thống kê tổng quan cho dashboard
        /// User: Chỉ xem được dữ liệu của mình
        /// Admin: Xem được dữ liệu của bất kỳ user nào
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStatistics([FromQuery] int? userId)
        {
            try
            {
                var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(currentUserIdClaim))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "User not authenticated",
                        401));
                }

                var currentUserId = int.Parse(currentUserIdClaim);

                // Authorization logic
                int targetUserId;
                if (userRole == "ADMIN")
                {
                    // Admin có thể xem dashboard của user khác
                    targetUserId = userId ?? currentUserId;
                }
                else
                {
                    // User thường chỉ xem được của mình
                    // Validate: Nếu truyền userId khác với token → Forbidden
                    if (userId.HasValue && userId.Value != currentUserId)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            $"User role can only view their own data. Your userId is {currentUserId}, but you requested userId {userId.Value}",
                            403));
                    }
                    targetUserId = currentUserId;
                }

                var statistics = await _reportService.GetDashboardStatisticsAsync(targetUserId);
                
                return Ok(ApiResponse<DashboardStatisticsDto>.SuccessResponse(
                    statistics,
                    "Dashboard statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Failed to retrieve dashboard statistics",
                    400));
            }
        }

        /// <summary>
        /// GET /api/statistics/trends
        /// Lấy xu hướng theo thời gian
        /// User: Chỉ xem được dữ liệu của mình
        /// Admin: Xem được dữ liệu của bất kỳ user nào
        /// </summary>
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends(
            [FromQuery] int? userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string period = "daily")
        {
            try
            {
                var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(currentUserIdClaim))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "User not authenticated",
                        401));
                }

                var currentUserId = int.Parse(currentUserIdClaim);

                // Authorization logic
                int? targetUserId;
                if (userRole == "ADMIN")
                {
                    targetUserId = userId;
                }
                else
                {
                    // User thường chỉ xem được của mình
                    // Validate: Nếu truyền userId khác với token → Forbidden
                    if (userId.HasValue && userId.Value != currentUserId)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            $"User role can only view their own data. Your userId is {currentUserId}, but you requested userId {userId.Value}",
                            403));
                    }
                    targetUserId = currentUserId;
                }

                // Validate period
                var validPeriods = new[] { "daily", "weekly", "monthly" };
                if (!validPeriods.Contains(period.ToLower()))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Invalid period",
                        "Period must be one of: daily, weekly, monthly",
                        400));
                }

                var request = new TrendReportRequest
                {
                    UserId = targetUserId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Period = period
                };

                var trendReport = await _reportService.GetTrendReportAsync(request);
                
                return Ok(ApiResponse<TrendReportDto>.SuccessResponse(
                    trendReport,
                    "Trend report retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Failed to retrieve trend report",
                    400));
            }
        }
    }
}
