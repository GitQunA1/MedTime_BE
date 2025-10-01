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
    [Route("api/prescriptionschedule")]
    public class PrescriptionscheduleController : ControllerBase
    {
        private readonly PrescriptionscheduleService _service;

        public PrescriptionscheduleController(PrescriptionscheduleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách prescription schedules có phân trang
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

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize);
            return Ok(ApiResponse<PaginatedResult<PrescriptionscheduleDto>>.SuccessResponse(
                paginatedResult,
                "Prescription schedules retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin prescription schedule theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not find prescription schedule with given ID",
                    404));
            }

            return Ok(ApiResponse<PrescriptionscheduleDto>.SuccessResponse(dto, "Prescription schedule retrieved successfully"));
        }

        /// <summary>
        /// Tạo prescription schedule mới
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

            var createdDto = await _service.CreateAsync(request);
            return Ok(ApiResponse<PrescriptionscheduleDto>.SuccessResponse(
                createdDto,
                "Prescription schedule created successfully",
                201));
        }

        /// <summary>
        /// Cập nhật prescription schedule
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

            var result = await _service.UpdateAsync(id, request);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not update prescription schedule because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule updated successfully"));
        }

        /// <summary>
        /// Xóa prescription schedule
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not delete prescription schedule because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule deleted successfully"));
        }
    }
}
