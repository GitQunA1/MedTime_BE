using MedTime.Data;
using MedTime.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class DevicetokenRepo : BaseRepo<Devicetoken, int>
    {
        private readonly MedTimeDBContext _context;
        public DevicetokenRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả active tokens của user
        /// </summary>
        public async Task<List<Devicetoken>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _context.Devicetokens
                .Where(t => t.Userid == userId && t.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy token theo userId và token string
        /// </summary>
        public async Task<Devicetoken?> GetByUserIdAndTokenAsync(int userId, string token)
        {
            return await _context.Devicetokens
                .FirstOrDefaultAsync(t => t.Userid == userId && t.Token == token);
        }

        /// <summary>
        /// Deactivate tất cả tokens của user (khi logout all devices)
        /// </summary>
        public async Task DeactivateAllUserTokensAsync(int userId)
        {
            var tokens = await _context.Devicetokens
                .Where(t => t.Userid == userId && t.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsActive = false;
                token.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa token cụ thể
        /// </summary>
        public async Task<bool> DeleteByTokenAsync(string token)
        {
            var entity = await _context.Devicetokens
                .FirstOrDefaultAsync(t => t.Token == token);
            
            if (entity == null) return false;

            _context.Devicetokens.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
