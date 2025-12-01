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
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly GuardianlinkService _guardianlinkService;

        public ReportController(ReportService reportService, GuardianlinkService guardianlinkService)
        {
            _reportService = reportService;
            _guardianlinkService = guardianlinkService;
        }

        /// <summary>
        /// GET /api/report/adherence
        /// Lấy báo cáo tuân thủ uống thuốc với filtering linh hoạt
        /// User: Xem dữ liệu của mình hoặc của patient mà mình là guardian
        /// Admin: Xem được dữ liệu của bất kỳ user nào
        /// </summary>
        [HttpGet("adherence")]
        public async Task<IActionResult> GetAdherenceReport(
            [FromQuery] int? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? medicineId)
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
                    // Admin có thể xem user bất kỳ hoặc tất cả (userId = null)
                    targetUserId = userId;
                }
                else if (userId.HasValue && userId.Value != currentUserId)
                {
                    // User muốn xem data của người khác - check guardian relationship
                    var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, userId.Value);
                    if (!isGuardian)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            $"You can only view your own data or data of patients you are guardian of. Your userId is {currentUserId}, but you requested userId {userId.Value}",
                            403));
                    }
                    targetUserId = userId.Value;
                }
                else
                {
                    // User xem data của chính mình
                    targetUserId = currentUserId;
                }

                var request = new AdherenceReportRequest
                {
                    UserId = targetUserId,
                    StartDate = startDate,
                    EndDate = endDate,
                    MedicineId = medicineId
                };

                var report = await _reportService.GetAdherenceReportAsync(request);
                
                return Ok(ApiResponse<AdherenceReportDto>.SuccessResponse(
                    report,
                    "Adherence report retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Failed to retrieve adherence report",
                    400));
            }
        }

        /// <summary>
        /// GET /api/report/missed-doses
        /// Lấy báo cáo số lần bỏ uống thuốc
        /// User: Xem dữ liệu của mình hoặc của patient mà mình là guardian
        /// Admin: Xem được dữ liệu của bất kỳ user nào
        /// </summary>
        [HttpGet("missed-doses")]
        public async Task<IActionResult> GetMissedDosesReport(
            [FromQuery] int? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
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
                else if (userId.HasValue && userId.Value != currentUserId)
                {
                    // User muốn xem data của người khác - check guardian relationship
                    var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, userId.Value);
                    if (!isGuardian)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            $"You can only view your own data or data of patients you are guardian of. Your userId is {currentUserId}, but you requested userId {userId.Value}",
                            403));
                    }
                    targetUserId = userId.Value;
                }
                else
                {
                    targetUserId = currentUserId;
                }

                var request = new MissedDosesRequest
                {
                    UserId = targetUserId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var report = await _reportService.GetMissedDosesReportAsync(request);
                
                return Ok(ApiResponse<MissedDosesReportDto>.SuccessResponse(
                    report,
                    "Missed doses report retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Failed to retrieve missed doses report",
                    400));
            }
        }

        /// <summary>
        /// GET /api/report/medicine-usage
        /// Lấy báo cáo sử dụng thuốc theo loại
        /// User: Xem dữ liệu của mình hoặc của patient mà mình là guardian
        /// Admin: Xem được dữ liệu của bất kỳ user nào
        /// </summary>
        [HttpGet("medicine-usage")]
        public async Task<IActionResult> GetMedicineUsageReport(
            [FromQuery] int? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
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
                else if (userId.HasValue && userId.Value != currentUserId)
                {
                    // User muốn xem data của người khác - check guardian relationship
                    var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, userId.Value);
                    if (!isGuardian)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            $"You can only view your own data or data of patients you are guardian of. Your userId is {currentUserId}, but you requested userId {userId.Value}",
                            403));
                    }
                    targetUserId = userId.Value;
                }
                else
                {
                    targetUserId = currentUserId;
                }

                var request = new MedicineUsageRequest
                {
                    UserId = targetUserId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var report = await _reportService.GetMedicineUsageReportAsync(request);
                
                return Ok(ApiResponse<MedicineUsageReportDto>.SuccessResponse(
                    report,
                    "Medicine usage report retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Failed to retrieve medicine usage report",
                    400));
            }
        }
    }
}
