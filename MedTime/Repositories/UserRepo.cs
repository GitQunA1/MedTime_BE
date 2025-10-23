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
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Ispremium = isPremium;
            user.Premiumstart = premiumStart;
            user.Premiumend = premiumEnd;
            user.Updatedat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
