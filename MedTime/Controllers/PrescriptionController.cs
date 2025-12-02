using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace MedTime.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/prescription")]
    public class PrescriptionController : ControllerBase
    {
        private readonly PrescriptionService _service;
        private readonly GuardianlinkService _guardianlinkService;

        public PrescriptionController(PrescriptionService service, GuardianlinkService guardianlinkService)
        {
            _service = service;
            _guardianlinkService = guardianlinkService;
        }

        /// <summary>
        /// Lấy danh sách prescriptions có phân trang
        /// USER: Xem prescription của chính mình hoặc của patient mà mình là guardian
        /// ADMIN: Xem tất cả
        /// Query param: ?patientId=xxx để xem prescription của patient (nếu là guardian)
        /// </summary>
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

            if (userRole == "ADMIN")
            {
                // ADMIN: Lấy tất cả hoặc theo patientId nếu có
                filterUserId = patientId;
            }
            else if (patientId.HasValue)
            {
                // USER muốn xem prescription của patient khác
                // Check xem có phải guardian của patient đó không
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, patientId.Value);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You are not authorized to view this patient's prescriptions. You must be their guardian.",
                        403));
                }
                filterUserId = patientId.Value;
            }
            else
            {
                // USER: Chỉ lấy của chính mình
                filterUserId = currentUserId;
            }

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId);
            return Ok(ApiResponse<PaginatedResult<PrescriptionDto>>.SuccessResponse(
                paginatedResult,
                "Prescriptions retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin prescription theo ID
        /// USER: Xem prescription của chính mình hoặc của patient mà mình là guardian
        /// ADMIN: Xem tất cả
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription not found",
                    "Could not find prescription with given ID",
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

            // Kiểm tra quyền: ADMIN, chính user đó, hoặc guardian của user đó
            if (userRole != "ADMIN" && dto.Userid != currentUserId)
            {
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, dto.Userid);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to view this prescription",
                        403));
                }
            }

            return Ok(ApiResponse<PrescriptionDto>.SuccessResponse(dto, "Prescription retrieved successfully"));
        }

        /// <summary>
        /// Tạo prescription mới
        /// USER: Tạo cho chính mình
        /// GUARDIAN: Có thể tạo cho patient bằng cách truyền patientId
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] PrescriptionCreate request, [FromQuery] int? patientId = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);
            int targetUserId;

            if (patientId.HasValue && patientId.Value != currentUserId)
            {
                // Guardian muốn tạo prescription cho patient
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, patientId.Value);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You are not authorized to create prescriptions for this patient. You must be their guardian.",
                        403));
                }
                targetUserId = patientId.Value;
            }
            else
            {
                // Tạo cho chính mình
                targetUserId = currentUserId;
            }

            try
            {
                var createdDto = await _service.CreateAsync(request, targetUserId);

                return Ok(ApiResponse<PrescriptionDto>.SuccessResponse(
                    createdDto,
                    "Prescription created successfully",
                    201));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "Forbidden",
                    403));
            }
        }

        /// <summary>
        /// Cập nhật prescription
        /// USER: Chỉ update được prescription của chính mình
        /// GUARDIAN: Có thể update prescription của patient mà mình là guardian
        /// ADMIN: Update tất cả
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] PrescriptionUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            // Kiểm tra quyền trước khi update
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription not found",
                    "Could not update prescription because it does not exist",
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

            // Kiểm tra quyền: ADMIN, chính user đó, hoặc guardian của user đó
            if (userRole != "ADMIN" && existing.Userid != currentUserId)
            {
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, existing.Userid);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to update this prescription. Only the owner or their guardian can modify it.",
                        403));
                }
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription updated successfully"));
        }

        /// <summary>
        /// Xóa prescription
        /// USER: Chỉ xóa được prescription của chính mình
        /// GUARDIAN: Có thể xóa prescription của patient mà mình là guardian
        /// ADMIN: Xóa tất cả
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            // Kiểm tra quyền trước khi xóa
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription not found",
                    "Could not delete prescription because it does not exist",
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

            // Kiểm tra quyền: ADMIN, chính user đó, hoặc guardian của user đó
            if (userRole != "ADMIN" && existing.Userid != currentUserId)
            {
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, existing.Userid);
                if (!isGuardian)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to delete this prescription. Only the owner or their guardian can delete it.",
                        403));
                }
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription deleted successfully"));
        }
    }
}
