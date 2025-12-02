using MedTime.Data;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class PaymenthistoryRepo : BaseRepo<Paymenthistory, int>
    {
        private readonly MedTimeDBContext _context;

        public PaymenthistoryRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Paymenthistory?> GetByOrderIdAsync(string orderId)
        {
            return await _context.Paymenthistories
                .Include(p => p.Plan)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Orderid == orderId);
        }

        public async Task<List<Paymenthistory>> GetUserPaymentHistoryAsync(int userId)
        {
            return await _context.Paymenthistories
                .Include(p => p.Plan)
                .Where(p => p.Userid == userId)
                .OrderByDescending(p => p.Createdat)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Paymenthistory?> GetLatestSuccessfulPaymentAsync(int userId)
        {
            return await _context.Paymenthistories
                .Include(p => p.Plan)
                .Where(p => p.Userid == userId && p.Status == PaymentStatusEnum.PAID)
                .OrderByDescending(p => p.Paidat)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdatePaymentStatusAsync(
            string orderId, 
            PaymentStatusEnum status, 
            string? transactionId = null,
            string? payosResponse = null)
        {
            var payment = await _context.Paymenthistories
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Orderid == orderId);

            if (payment == null) return false;

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            payment.Status = status;
            payment.Transactionid = transactionId;
            payment.Payosresponse = payosResponse;
            payment.Updatedat = now;

            if (status == PaymentStatusEnum.PAID)
            {
                payment.Paidat = now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<Paymenthistory> GetAnalyticsQuery(DateTime? from, DateTime? to)
        {
            var query = _context.Paymenthistories
                .Include(p => p.Plan)
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            if (from.HasValue)
            {
                var fromValue = from.Value;
                query = query.Where(p =>
                    (p.Paidat.HasValue && p.Paidat.Value >= fromValue) ||
                    (!p.Paidat.HasValue && p.Createdat.HasValue && p.Createdat.Value >= fromValue));
            }

            if (to.HasValue)
            {
                var toValue = to.Value;
                query = query.Where(p =>
                    (p.Paidat.HasValue && p.Paidat.Value <= toValue) ||
                    (!p.Paidat.HasValue && p.Createdat.HasValue && p.Createdat.Value <= toValue));
            }

            return query;
        }
    }
}
