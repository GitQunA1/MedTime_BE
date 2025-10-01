using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedTime.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/guardianlink")]
    public class GuardianlinkController : ControllerBase
    {
        private readonly GuardianlinkService _service;

        public GuardianlinkController(GuardianlinkService service)
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
            return Ok(ApiResponse<PaginatedResult<GuardianlinkDto>>.SuccessResponse(
                paginatedResult,
                "Guardian links retrieved successfully"));
        }

        [HttpGet("{guardianId}/{patientId}")]
        public async Task<IActionResult> GetByIdAsync(int guardianId, int patientId)
        {
            var dto = await _service.GetByIdAsync(guardianId, patientId);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Guardian link not found",
                    "Could not find guardian link with given IDs",
                    404));
            }

            return Ok(ApiResponse<GuardianlinkDto>.SuccessResponse(dto, "Guardian link retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] GuardianlinkCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var createdDto = await _service.CreateAsync(request);
            return Ok(ApiResponse<GuardianlinkDto>.SuccessResponse(
                createdDto,
                "Guardian link created successfully",
                201));
        }

        [HttpPut("{guardianId}/{patientId}")]
        public async Task<IActionResult> UpdateAsync(int guardianId, int patientId, [FromBody] GuardianlinkUpdate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var result = await _service.UpdateAsync(guardianId, patientId, request);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Guardian link not found",
                    "Could not update guardian link because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Guardian link updated successfully"));
        }

        [HttpDelete("{guardianId}/{patientId}")]
        public async Task<IActionResult> DeleteAsync(int guardianId, int patientId)
        {
            var result = await _service.DeleteAsync(guardianId, patientId);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Guardian link not found",
                    "Could not delete guardian link because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Guardian link deleted successfully"));
        }
    }
}
