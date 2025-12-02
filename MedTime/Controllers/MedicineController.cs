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
    [Route("api/medicine")]
    public class MedicineController : ControllerBase
    {
        private readonly MedicineService _service;

        public MedicineController(MedicineService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách medicines có phân trang
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
            return Ok(ApiResponse<PaginatedResult<MedicineDto>>.SuccessResponse(
                paginatedResult,
                "Medicines retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin medicine theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Medicine not found",
                    "Could not find medicine with given ID",
                    404));
            }

            return Ok(ApiResponse<MedicineDto>.SuccessResponse(dto, "Medicine retrieved successfully"));
        }

        /// <summary>
        /// Tạo medicine mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] MedicineCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            var createdDto = await _service.CreateAsync(request);
            return Ok(ApiResponse<MedicineDto>.SuccessResponse(
                createdDto,
                "Medicine created successfully",
                201));
        }

        /// <summary>
        /// Cập nhật medicine
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] MedicineUpdate request)
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
                    "Medicine not found",
                    "Could not update medicine because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Medicine updated successfully"));
        }

        /// <summary>
        /// Xóa medicine
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Medicine not found",
                    "Could not delete medicine because it does not exist",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Medicine deleted successfully"));
        }
    }
}
