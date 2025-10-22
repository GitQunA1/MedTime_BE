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
    [Route("api/prescriptionschedule")]
    public class PrescriptionscheduleController : ControllerBase
    {
        private readonly PrescriptionscheduleService _service;
        private readonly PrescriptionService _prescriptionService;

        public PrescriptionscheduleController(
            PrescriptionscheduleService service,
            PrescriptionService prescriptionService)
        {
            _service = service;
            _prescriptionService = prescriptionService;
        }

        /// <summary>
        /// Lấy danh sách prescription schedules có phân trang
        /// USER: Chỉ xem được schedule của prescription thuộc về mình (author)
        /// ADMIN: Xem tất cả
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationRequest pagination)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            // ADMIN: Xem tất cả, USER: Chỉ xem schedule của prescription mình tạo (author)
            int? filterUserId = (userRole == "ADMIN") ? null : int.Parse(userIdClaim);

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId);
            return Ok(ApiResponse<PaginatedResult<PrescriptionscheduleDto>>.SuccessResponse(
                paginatedResult,
                "Prescription schedules retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin prescription schedule theo ID
        /// USER: Chỉ xem được schedule của prescription thuộc về mình (author)
        /// ADMIN: Xem tất cả
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not find prescription schedule with given ID",
                    404));
            }

            // Check ownership qua Prescription.Userid (chỉ author mới xem được)
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, int.Parse(userIdClaim));
                if (!hasAccess)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to view this schedule. Only the prescription author can access it.",
                        403));
                }
            }

            return Ok(ApiResponse<PrescriptionscheduleDto>.SuccessResponse(dto, "Prescription schedule retrieved successfully"));
        }

        /// <summary>
        /// Tạo prescription schedule mới
        /// USER: Chỉ tạo schedule cho prescription của chính mình (author)
        /// ADMIN: Tạo cho bất kỳ prescription nào
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] PrescriptionscheduleCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            // Kiểm tra prescription có tồn tại không
            var prescription = await _prescriptionService.GetByIdAsync(request.Prescriptionid);
            if (prescription == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription not found",
                    "Cannot create schedule for non-existent prescription",
                    404));
            }

            // USER: Chỉ tạo schedule cho prescription của chính mình (author)
            if (userRole != "ADMIN")
            {
                var isOwner = await _prescriptionService.CheckPrescriptionOwnershipAsync(
                    request.Prescriptionid, 
                    int.Parse(userIdClaim));
                
                if (!isOwner)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to create schedule for this prescription. Only the prescription author can do this.",
                        403));
                }
            }

            var createdDto = await _service.CreateAsync(request);
            return Ok(ApiResponse<PrescriptionscheduleDto>.SuccessResponse(
                createdDto,
                "Prescription schedule created successfully",
                201));
        }

        /// <summary>
        /// Cập nhật prescription schedule
        /// USER: Chỉ update được schedule của prescription thuộc về mình (author)
        /// ADMIN: Update tất cả
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] PrescriptionscheduleUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not update prescription schedule because it does not exist",
                    404));
            }

            // USER: Chỉ update được schedule của prescription mình tạo (author)
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, int.Parse(userIdClaim));
                if (!hasAccess)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to update this schedule. Only the prescription author can modify it.",
                        403));
                }
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule updated successfully"));
        }

        /// <summary>
        /// Xóa prescription schedule
        /// USER: Chỉ xóa được schedule của prescription thuộc về mình (author)
        /// ADMIN: Xóa tất cả
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not delete prescription schedule because it does not exist",
                    404));
            }

            // USER: Chỉ xóa được schedule của prescription mình tạo (author)
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, int.Parse(userIdClaim));
                if (!hasAccess)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You do not have permission to delete this schedule. Only the prescription author can delete it.",
                        403));
                }
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule deleted successfully"));
        }
    }
}
