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
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Lấy danh sách các gói Premium
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans()
        {
            try
            {
                var plans = await _paymentService.GetActivePlansAsync();
                return Ok(ApiResponse<List<PremiumplanDto>>.SuccessResponse(
                    plans,
                    "Premium plans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }

        /// <summary>
        /// Tạo link thanh toán PayOS
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    400));
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "Invalid user token",
                        401));
                }

                var response = await _paymentService.CreatePaymentLinkAsync(userId, request);
                
                if (response == null)
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResponse(
                        "Payment Error",
                        "Failed to create payment link",
                        500));
                }

                return Ok(ApiResponse<CreatePaymentResponse>.SuccessResponse(
                    response,
                    "Payment link created successfully. Please complete payment at the checkout URL."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán
        /// </summary>
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(string orderId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "Invalid user token",
                        401));
                }

                var status = await _paymentService.GetPaymentStatusAsync(orderId);
                
                if (status == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Not Found",
                        $"Payment with orderId {orderId} not found",
                        404));
                }

                return Ok(ApiResponse<PaymentStatusResponse>.SuccessResponse(
                    status,
                    "Payment status retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }

        /// <summary>
        /// Webhook callback từ PayOS
        /// Endpoint này sẽ được PayOS gọi khi thanh toán thành công/thất bại
        /// </summary>
        [HttpPost("payos-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSCallback([FromBody] PayOSWebhookRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Signature) || request.Data == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Bad Request",
                        "Invalid webhook data",
                        400));
                }

                var processed = await _paymentService.HandlePayOSWebhookAsync(request);

                if (!processed)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Bad Request",
                        "Webhook signature verification failed",
                        400));
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { },
                    "Webhook processed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }

        /// <summary>
        /// Lấy lịch sử thanh toán của user
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "Invalid user token",
                        401));
                }

                var history = await _paymentService.GetPaymentHistoryAsync(userId);
                
                return Ok(ApiResponse<List<PaymenthistoryDto>>.SuccessResponse(
                    history,
                    "Payment history retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }

        /// <summary>
        /// Hủy Premium subscription
        /// </summary>
        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Unauthorized",
                        "Invalid user token",
                        401));
                }

                var success = await _paymentService.CancelSubscriptionAsync(userId);
                
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Not Found",
                        "User not found or already cancelled",
                        404));
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { },
                    "Premium subscription cancelled successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Internal Server Error",
                    ex.Message,
                    500));
            }
        }
    }
}
