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
    [Route("api/prescription")]
    public class PrescriptionController : ControllerBase
    {
        private readonly PrescriptionService _service;

        public PrescriptionController(PrescriptionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách prescriptions có phân trang
        /// USER: Chỉ xem được prescription của chính mình
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

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            // ADMIN: Lấy tất cả, USER: Chỉ lấy của chính mình (author)
            int? filterUserId = (userRole == "ADMIN") ? null : int.Parse(userIdClaim);

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId);
            return Ok(ApiResponse<PaginatedResult<PrescriptionDto>>.SuccessResponse(
                paginatedResult,
                "Prescriptions retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin prescription theo ID
        /// USER: Chỉ xem được prescription của chính mình (author)
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

            // Kiểm tra quyền: ADMIN hoặc chính user đó (author)
            if (userRole != "ADMIN" && dto.Userid != int.Parse(userIdClaim))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(
                    "Forbidden",
                    "You do not have permission to view this prescription",
                    403));
            }

            return Ok(ApiResponse<PrescriptionDto>.SuccessResponse(dto, "Prescription retrieved successfully"));
        }

        /// <summary>
        /// Tạo prescription mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] PrescriptionCreate request)
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

            var userId = int.Parse(userIdClaim);
            var createdDto = await _service.CreateAsync(request, userId);

            return Ok(ApiResponse<PrescriptionDto>.SuccessResponse(
                createdDto,
                "Prescription created successfully",
                201));
        }

        /// <summary>
        /// Cập nhật prescription
        /// USER: Chỉ update được prescription của chính mình (author)
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

            // Chỉ author hoặc admin mới được update
            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(
                    "Forbidden",
                    "You do not have permission to update this prescription. Only the author can modify it.",
                    403));
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription updated successfully"));
        }

        /// <summary>
        /// Xóa prescription
        /// USER: Chỉ xóa được prescription của chính mình (author)
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

            // Chỉ author hoặc admin mới được xóa
            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(
                    "Forbidden",
                    "You do not have permission to delete this prescription. Only the author can delete it.",
                    403));
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription deleted successfully"));
        }
    }
}
