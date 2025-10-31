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
    }
}
