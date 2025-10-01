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
    [Route("api/emergencycontact")]
    public class EmergencycontactController : ControllerBase
    {
        private readonly EmergencycontactService _service;

        public EmergencycontactController(EmergencycontactService service)
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

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PaginatedResult<EmergencycontactDto>>.SuccessResponse(
                paginatedResult,
                "Emergency contacts retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Emergency contact not found",
                    "Could not find emergency contact with given ID",
                    404));
            }

            return Ok(ApiResponse<EmergencycontactDto>.SuccessResponse(dto, "Emergency contact retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] EmergencycontactCreate request)
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
            
            return Ok(ApiResponse<EmergencycontactDto>.SuccessResponse(
                createdDto,
                "Emergency contact created successfully",
                201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] EmergencycontactUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var result = await _service.UpdateAsync(id, request);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Emergency contact not found",
                    "Could not update emergency contact because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Emergency contact updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Emergency contact not found",
                    "Could not delete emergency contact because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Emergency contact deleted successfully"));
        }
    }
}
