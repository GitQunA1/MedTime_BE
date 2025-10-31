using MedTime.Data;
using MedTime.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class UserRepo : BaseRepo<User, int>
    {
        private readonly MedTimeDBContext _context;
        public UserRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> UpdatePremiumStatusAsync(int userId, bool isPremium, DateTime? premiumStart, DateTime? premiumEnd)
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return false;

            DateTime? ConvertToUnspecified(DateTime? value)
            {
                return value.HasValue
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified)
                    : null;
            }

            user.Ispremium = isPremium;
            user.Premiumstart = ConvertToUnspecified(premiumStart);
            user.Premiumend = ConvertToUnspecified(premiumEnd);
            user.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
