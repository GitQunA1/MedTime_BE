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
        private readonly GuardianlinkService _guardianlinkService;

        public PrescriptionscheduleController(
            PrescriptionscheduleService service,
            PrescriptionService prescriptionService,
            GuardianlinkService guardianlinkService)
        {
            _service = service;
            _prescriptionService = prescriptionService;
            _guardianlinkService = guardianlinkService;
        }

        /// <summary>
        /// Lấy danh sách prescription schedules có phân trang
        /// USER: Xem schedule của prescription của mình hoặc của patients mà mình là guardian
        /// ADMIN: Xem tất cả
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
            List<int>? additionalUserIds = null;

            if (userRole == "ADMIN")
            {
                // ADMIN: Xem tất cả hoặc theo patientId
                filterUserId = patientId;
            }
            else if (patientId.HasValue)
            {
                // User muốn xem schedule của patient cụ thể
                var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, patientId.Value);
                if (!isGuardian && patientId.Value != currentUserId)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse(
                        "Forbidden",
                        "You are not authorized to view this patient's schedules. You must be their guardian.",
                        403));
                }
                filterUserId = patientId.Value;
            }
            else
            {
                // USER: Xem schedule của mình và của patients mà mình là guardian
                filterUserId = currentUserId;
                additionalUserIds = await _guardianlinkService.GetPatientIdsByGuardianAsync(currentUserId);
            }

            var paginatedResult = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize, filterUserId, additionalUserIds);
            return Ok(ApiResponse<PaginatedResult<PrescriptionscheduleDto>>.SuccessResponse(
                paginatedResult,
                "Prescription schedules retrieved successfully"));
        }

        /// <summary>
        /// Lấy thông tin prescription schedule theo ID
        /// USER: Xem schedule của prescription của mình hoặc của patients mà mình là guardian
        /// ADMIN: Xem tất cả
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);

            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not find prescription schedule with given ID",
                    404));
            }

            // Check ownership qua Prescription.Userid hoặc guardian relationship
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, currentUserId);
                if (!hasAccess)
                {
                    // Check guardian relationship
                    var ownerUserId = await _service.GetScheduleOwnerUserIdAsync(id);
                    if (ownerUserId.HasValue)
                    {
                        var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, ownerUserId.Value);
                        if (!isGuardian)
                        {
                            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                                "Forbidden",
                                "You do not have permission to view this schedule. Only the prescription owner or their guardian can access it.",
                                403));
                        }
                    }
                    else
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            "You do not have permission to view this schedule.",
                            403));
                    }
                }
            }

            return Ok(ApiResponse<PrescriptionscheduleDto>.SuccessResponse(dto, "Prescription schedule retrieved successfully"));
        }

        /// <summary>
        /// Tạo prescription schedule mới
        /// USER: Tạo schedule cho prescription của mình hoặc của patients mà mình là guardian
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

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);

            // Kiểm tra prescription có tồn tại không
            var prescription = await _prescriptionService.GetByIdAsync(request.Prescriptionid);
            if (prescription == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription not found",
                    "Cannot create schedule for non-existent prescription",
                    404));
            }

            // USER: Tạo schedule cho prescription của mình hoặc của patient mà mình là guardian
            if (userRole != "ADMIN")
            {
                var isOwner = await _prescriptionService.CheckPrescriptionOwnershipAsync(
                    request.Prescriptionid, 
                    currentUserId);
                
                if (!isOwner)
                {
                    // Check guardian relationship
                    var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, prescription.Userid);
                    if (!isGuardian)
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            "You do not have permission to create schedule for this prescription. Only the prescription owner or their guardian can do this.",
                            403));
                    }
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
        /// USER: Update schedule của prescription của mình hoặc của patients mà mình là guardian
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

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not update prescription schedule because it does not exist",
                    404));
            }

            // USER: Update schedule của prescription mình hoặc của patient mà mình là guardian
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, currentUserId);
                if (!hasAccess)
                {
                    // Check guardian relationship
                    var ownerUserId = await _service.GetScheduleOwnerUserIdAsync(id);
                    if (ownerUserId.HasValue)
                    {
                        var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, ownerUserId.Value);
                        if (!isGuardian)
                        {
                            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                                "Forbidden",
                                "You do not have permission to update this schedule. Only the prescription owner or their guardian can modify it.",
                                403));
                        }
                    }
                    else
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            "You do not have permission to update this schedule.",
                            403));
                    }
                }
            }

            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule updated successfully"));
        }

        /// <summary>
        /// Xóa prescription schedule
        /// USER: Xóa schedule của prescription của mình hoặc của patients mà mình là guardian
        /// ADMIN: Xóa tất cả
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Unauthorized", "User not logged in", 401));
            }

            var currentUserId = int.Parse(userIdClaim);

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Prescription schedule not found",
                    "Could not delete prescription schedule because it does not exist",
                    404));
            }

            // USER: Xóa schedule của prescription mình hoặc của patient mà mình là guardian
            if (userRole != "ADMIN")
            {
                var hasAccess = await _service.CheckUserAccessAsync(id, currentUserId);
                if (!hasAccess)
                {
                    // Check guardian relationship
                    var ownerUserId = await _service.GetScheduleOwnerUserIdAsync(id);
                    if (ownerUserId.HasValue)
                    {
                        var isGuardian = await _guardianlinkService.IsGuardianOfPatientAsync(currentUserId, ownerUserId.Value);
                        if (!isGuardian)
                        {
                            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                                "Forbidden",
                                "You do not have permission to delete this schedule. Only the prescription owner or their guardian can delete it.",
                                403));
                        }
                    }
                    else
                    {
                        return StatusCode(403, ApiResponse<object>.ErrorResponse(
                            "Forbidden",
                            "You do not have permission to delete this schedule.",
                            403));
                    }
                }
            }

            var result = await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Prescription schedule deleted successfully"));
        }
    }
}
