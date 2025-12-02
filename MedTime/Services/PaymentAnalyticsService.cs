using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Services
{
    public class PaymentAnalyticsService
    {
        private readonly PaymenthistoryRepo _paymentRepo;

        public PaymentAnalyticsService(PaymenthistoryRepo paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<PaymentSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            var summary = await query
                .GroupBy(_ => true)
                .Select(g => new PaymentSummaryDto
                {
                    TotalRevenue = g.Where(p => p.Status == PaymentStatusEnum.PAID).Sum(p => (decimal?)p.Amount) ?? 0,
                    TotalTransactions = g.Count(),
                    PaidTransactions = g.Count(p => p.Status == PaymentStatusEnum.PAID),
                    PendingTransactions = g.Count(p => p.Status == PaymentStatusEnum.PENDING),
                    FailedTransactions = g.Count(p => p.Status == PaymentStatusEnum.FAILED),
                    CancelledTransactions = g.Count(p => p.Status == PaymentStatusEnum.CANCELLED),
                    AverageRevenuePerPaidTransaction = g.Where(p => p.Status == PaymentStatusEnum.PAID).Average(p => (decimal?)p.Amount) ?? 0
                })
                .FirstOrDefaultAsync();

            if (summary == null)
            {
                return new PaymentSummaryDto();
            }

            summary.TotalRevenue = Math.Round(summary.TotalRevenue, 2);
            summary.AverageRevenuePerPaidTransaction = Math.Round(summary.AverageRevenuePerPaidTransaction, 2);
            summary.PaidConversionRate = summary.TotalTransactions == 0
                ? 0
                : Math.Round((decimal)summary.PaidTransactions / summary.TotalTransactions * 100m, 2);

            return summary;
        }

        public async Task<List<PaymentDailyRevenueDto>> GetDailyRevenueAsync(DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            var daily = await query
                .Where(p => p.Status == PaymentStatusEnum.PAID && p.Paidat.HasValue)
                .GroupBy(p => p.Paidat!.Value.Date)
                .Select(g => new PaymentDailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    PaidTransactions = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            foreach (var item in daily)
            {
                item.Date = SpecifyUnspecified(item.Date);
                item.Revenue = Math.Round(item.Revenue, 2);
            }

            return daily;
        }

        public async Task<List<PaymentPlanBreakdownDto>> GetPlanBreakdownAsync(DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            var data = await query
                .Where(p => p.Status == PaymentStatusEnum.PAID)
                .GroupBy(p => new
                {
                    p.Planid,
                    p.Plan.Planname,
                    p.Plan.Discountpercent
                })
                .Select(g => new PaymentPlanBreakdownDto
                {
                    PlanId = g.Key.Planid,
                    PlanName = g.Key.Planname ?? string.Empty,
                    DiscountPercent = g.Key.Discountpercent,
                    Revenue = g.Sum(p => p.Amount),
                    Transactions = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            var totalRevenue = data.Sum(x => x.Revenue);

            foreach (var item in data)
            {
                item.Revenue = Math.Round(item.Revenue, 2);
                item.RevenueShare = totalRevenue > 0
                    ? Math.Round(item.Revenue / totalRevenue * 100m, 2)
                    : 0;
            }

            return data;
        }

        public async Task<List<PaymentStatusBreakdownDto>> GetStatusBreakdownAsync(DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            var breakdown = await query
                .GroupBy(p => p.Status)
                .Select(g => new PaymentStatusBreakdownDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var totalCount = breakdown.Sum(x => x.Count);

            foreach (var status in Enum.GetValues<PaymentStatusEnum>())
            {
                if (!breakdown.Any(x => x.Status == status))
                {
                    breakdown.Add(new PaymentStatusBreakdownDto
                    {
                        Status = status
                    });
                }
            }

            foreach (var item in breakdown)
            {
                item.Revenue = Math.Round(item.Revenue, 2);
                item.Percentage = totalCount == 0
                    ? 0
                    : Math.Round((decimal)item.Count / totalCount * 100m, 2);
            }

            return breakdown
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Status.ToString())
                .ToList();
        }

        public async Task<List<PaymentTopCustomerDto>> GetTopCustomersAsync(int limit, DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            limit = Math.Clamp(limit, 1, 100);

            var customers = await query
                .Where(p => p.Status == PaymentStatusEnum.PAID)
                .GroupBy(p => new
                {
                    p.Userid,
                    p.User.Fullname,
                    p.User.Email
                })
                .Select(g => new PaymentTopCustomerDto
                {
                    UserId = g.Key.Userid,
                    FullName = g.Key.Fullname,
                    Email = g.Key.Email,
                    TotalSpent = g.Sum(p => p.Amount),
                    Transactions = g.Count(),
                    LastPaymentAt = g.Max(p => p.Paidat ?? p.Createdat)
                })
                .OrderByDescending(x => x.TotalSpent)
                .ThenByDescending(x => x.Transactions)
                .Take(limit)
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.TotalSpent = Math.Round(customer.TotalSpent, 2);
                customer.LastPaymentAt = SpecifyUnspecified(customer.LastPaymentAt);
            }

            return customers;
        }

        public async Task<List<PaymentRecentTransactionDto>> GetRecentTransactionsAsync(int limit, DateTime? from, DateTime? to)
        {
            var query = BuildAnalyticsQuery(from, to);

            limit = Math.Clamp(limit, 1, 200);

            var recent = await query
                .OrderByDescending(p => p.Paidat ?? p.Createdat ?? p.Updatedat)
                .ThenByDescending(p => p.Paymentid)
                .Take(limit)
                .Select(p => new PaymentRecentTransactionDto
                {
                    PaymentId = p.Paymentid,
                    OrderId = p.Orderid,
                    Amount = p.Amount,
                    Status = p.Status,
                    TransactionId = p.Transactionid,
                    UserId = p.Userid,
                    UserFullName = p.User.Fullname,
                    UserEmail = p.User.Email,
                    PlanId = p.Planid,
                    PlanName = p.Plan.Planname,
                    CreatedAt = p.Createdat,
                    PaidAt = p.Paidat,
                    UpdatedAt = p.Updatedat
                })
                .ToListAsync();

            foreach (var item in recent)
            {
                item.Amount = Math.Round(item.Amount, 2);
                item.CreatedAt = SpecifyUnspecified(item.CreatedAt);
                item.PaidAt = SpecifyUnspecified(item.PaidAt);
                item.UpdatedAt = SpecifyUnspecified(item.UpdatedAt);
            }

            return recent;
        }

    private IQueryable<Paymenthistory> BuildAnalyticsQuery(DateTime? from, DateTime? to)
        {
            var normalizedFrom = SpecifyUnspecified(from);
            var normalizedTo = SpecifyUnspecified(to);
            return _paymentRepo.GetAnalyticsQuery(normalizedFrom, normalizedTo);
        }

        private static DateTime? SpecifyUnspecified(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
        }

        private static DateTime SpecifyUnspecified(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        }
    }
}
