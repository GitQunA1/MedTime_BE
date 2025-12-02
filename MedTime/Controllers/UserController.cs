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
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;

        public UserController(UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách users có phân trang (ADMIN only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationRequest pagination)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PaginatedResult<UserDto>>.SuccessResponse(
                paginatedResult,
                "Users retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// User thường chỉ xem được thông tin của chính mình
        /// ADMIN xem được tất cả
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Kiểm tra quyền: Chỉ ADMIN hoặc chính user đó mới xem được
            if (userRole != "ADMIN" && userIdClaim != id.ToString())
            {
                return Forbid(); // 403 Forbidden
            }

            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not find user with given ID",
                    404));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(dto, "User retrieved successfully"));
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// User chỉ update được thông tin của chính mình
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UserUpdate request)
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

            // Kiểm tra quyền: Chỉ ADMIN hoặc chính user đó mới update được
            if (userRole != "ADMIN" && userIdClaim != id.ToString())
            {
                return Forbid();
            }

            var result = await _service.UpdateAsync(id, request);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not update user because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "User updated successfully"));
        }

        /// <summary>
        /// Xóa user (ADMIN only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not delete user because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully"));
        }

        /// <summary>
        /// Cập nhật role của user (ADMIN only)
        /// Ví dụ: Nâng USER → ADMIN
        /// </summary>
        [HttpPatch("{id}/role")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateRoleAsync(int id, [FromBody] UpdateUserRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var result = await _service.UpdateRoleAsync(id, request.Role);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not update user role because user does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                null!, 
                $"User role updated to {request.Role} successfully"));
        }

        /// <summary>
        /// Cập nhật trạng thái Premium (ADMIN only hoặc sau khi thanh toán)
        /// </summary>
        [HttpPatch("{id}/premium")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdatePremiumAsync(int id, [FromBody] UpdatePremiumRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var result = await _service.UpdatePremiumAsync(id, request);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not update premium status because user does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                null!, 
                request.IsPremium ? "Premium activated successfully" : "Premium cancelled successfully"));
        }

        /// <summary>
        /// Lấy thông tin profile của user hiện tại (từ JWT)
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var userId = int.Parse(userIdClaim);
            var dto = await _service.GetByIdAsync(userId);
            
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User not found",
                    "Could not find current user",
                    404));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(dto, "Current user retrieved successfully"));
        }
    }
}
