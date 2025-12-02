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
    [Route("api/calllog")]
    public class CalllogController : ControllerBase
    {
        private readonly CalllogService _service;

        public CalllogController(CalllogService service)
        {
            _service = service;
        }

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

            int? filterUserId = (userRole == "ADMIN") ? null : int.Parse(userIdClaim!);

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId);
            return Ok(ApiResponse<PaginatedResult<CalllogDto>>.SuccessResponse(
                paginatedResult,
                "Call logs retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Call log not found",
                    "Could not find call log with given ID",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN" && dto.Userid != int.Parse(userIdClaim!))
            {
                return Forbid();
            }

            return Ok(ApiResponse<CalllogDto>.SuccessResponse(dto, "Call log retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CalllogCreate request)
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
            
            return Ok(ApiResponse<CalllogDto>.SuccessResponse(
                createdDto,
                "Call log created successfully",
                201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] CalllogUpdate request)
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
                    "Call log not found",
                    "Could not update call log because it does not exist",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim!))
            {
                return Forbid();
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Call log updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Call log not found",
                    "Could not delete call log because it does not exist",
                    404));
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN" && existing.Userid != int.Parse(userIdClaim!))
            {
                return Forbid();
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Call log deleted successfully"));
        }
    }
}
