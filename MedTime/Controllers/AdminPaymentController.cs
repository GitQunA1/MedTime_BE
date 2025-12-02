using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedTime.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/admin/payments")]
    public class AdminPaymentController : ControllerBase
    {
        private readonly PaymentAnalyticsService _analyticsService;

        public AdminPaymentController(PaymentAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Tổng quan doanh thu trong khoảng thời gian
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var summary = await _analyticsService.GetSummaryAsync(from, to);
            return Ok(ApiResponse<PaymentSummaryDto>.SuccessResponse(summary, "Payment summary retrieved successfully"));
        }

        /// <summary>
        /// Doanh thu theo ngày trong khoảng thời gian
        /// </summary>
        [HttpGet("daily-revenue")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var dailyRevenue = await _analyticsService.GetDailyRevenueAsync(from, to);
            return Ok(ApiResponse<List<PaymentDailyRevenueDto>>.SuccessResponse(dailyRevenue, "Daily revenue retrieved successfully"));
        }

        /// <summary>
        /// Doanh thu theo gói premium
        /// </summary>
        [HttpGet("plan-breakdown")]
        public async Task<IActionResult> GetPlanBreakdown([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var breakdown = await _analyticsService.GetPlanBreakdownAsync(from, to);
            return Ok(ApiResponse<List<PaymentPlanBreakdownDto>>.SuccessResponse(breakdown, "Plan breakdown retrieved successfully"));
        }

        /// <summary>
        /// Phân bố trạng thái thanh toán
        /// </summary>
        [HttpGet("status-breakdown")]
        public async Task<IActionResult> GetStatusBreakdown([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var breakdown = await _analyticsService.GetStatusBreakdownAsync(from, to);
            return Ok(ApiResponse<List<PaymentStatusBreakdownDto>>.SuccessResponse(breakdown, "Status breakdown retrieved successfully"));
        }

        /// <summary>
        /// Top khách hàng theo doanh thu
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int? limit, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var effectiveLimit = limit.GetValueOrDefault(5);
            if (effectiveLimit <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Limit must be greater than 0", "Invalid limit", 400));
            }

            var customers = await _analyticsService.GetTopCustomersAsync(effectiveLimit, from, to);
            return Ok(ApiResponse<List<PaymentTopCustomerDto>>.SuccessResponse(customers, "Top customers retrieved successfully"));
        }

        /// <summary>
        /// Danh sách giao dịch gần nhất
        /// </summary>
        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int? limit, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!ValidateDateRange(from, to, out var badRequest))
            {
                return badRequest!;
            }

            var effectiveLimit = limit.GetValueOrDefault(20);
            if (effectiveLimit <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Limit must be greater than 0", "Invalid limit", 400));
            }

            var transactions = await _analyticsService.GetRecentTransactionsAsync(effectiveLimit, from, to);
            return Ok(ApiResponse<List<PaymentRecentTransactionDto>>.SuccessResponse(transactions, "Recent transactions retrieved successfully"));
        }

        private bool ValidateDateRange(DateTime? from, DateTime? to, out IActionResult? badRequestResult)
        {
            badRequestResult = null;

            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                badRequestResult = BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid date range",
                    "'from' must be earlier than or equal to 'to'",
                    400));
                return false;
            }

            return true;
        }
    }
}
