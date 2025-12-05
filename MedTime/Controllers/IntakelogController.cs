using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedTime.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/intakelog")]
    public class IntakelogController : ControllerBase
    {
        private readonly IntakelogService _service;
        private readonly GuardianlinkService _guardianlinkService;

        public IntakelogController(IntakelogService service, GuardianlinkService guardianlinkService)
        {
            _service = service;
            _guardianlinkService = guardianlinkService;
        }

        /// <summary>
        /// Lấy danh sách intake logs có phân trang
        /// USER: Xem intake logs của mình hoặc của patients mà mình là guardian
        /// ADMIN: Xem tất cả
        /// </summary>
        /// <param name="pagination">Thông tin phân trang</param>
        /// <param name="patientId">Optional: ID của patient cụ thể (guardian xem patient của mình)</param>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationRequest pagination, [FromQuery] int? patientId = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);
            int? filterUserId;
            List<int>? additionalUserIds = null;

            if (userRole == "ADMIN")
            {
                // ADMIN: Xem tất cả hoặc theo patientId
                filterUserId = patientId;
            }
            else if (patientId.HasValue)
            {
                // User muốn xem intake logs của patient cụ thể
                if (patientId.Value == currentUserId)
                {
                    // Xem của chính mình
                    filterUserId = currentUserId;
                }
                else
                {
                    // Kiểm tra guardian relationship
                    var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, patientId.Value);
                    if (!isGuardian)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            "You are not authorized to view this patient's intake logs. You must be their guardian.",
                            403));
                    }
                    filterUserId = patientId.Value;
                }
            }
            else
            {
                // USER: Xem intake logs của mình và của patients mà mình là guardian
                filterUserId = currentUserId;
                additionalUserIds = await _guardianlinkService.GetPatientIdsByGuardianAsync(currentUserId);
            }

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId, additionalUserIds);
            return Ok(ApiResponse<PaginatedResult<IntakelogDto>>.SuccessResponse(
                paginatedResult,
                "Intake logs retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin intake log theo ID
        /// USER: Xem intake log của mình hoặc của patients mà mình là guardian
        /// ADMIN: Xem tất cả
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Intake log not found",
                    "Could not find intake log with given ID",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);

            if (userRole != "ADMIN" && dto.Userid != currentUserId)
            {
                // Check guardian relationship
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, dto.Userid);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to view this intake log. Only the owner or their guardian can access it.",
                        403));
                }
            }

            return Ok(ApiResponse<IntakelogDto>.SuccessResponse(dto, "Intake log retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IntakelogCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var userId = int.Parse(userIdClaim);
            var createdDto = await _service.CreateAsync(request, userId);
            
            return Ok(ApiResponse<IntakelogDto>.SuccessResponse(
                createdDto,
                "Intake log created successfully",
                201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] IntakelogUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Intake log not found",
                    "Could not update intake log because it does not exist",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim!))
            {
                return Forbid();
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Intake log updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Intake log not found",
                    "Could not delete intake log because it does not exist",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim!))
            {
                return Forbid();
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Intake log deleted successfully"));
        }
    }
}
