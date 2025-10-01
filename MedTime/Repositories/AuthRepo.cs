using MedTime.Data;
using MedTime.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class AuthRepo
    {
        private readonly MedTimeDBContext _context;

        public AuthRepo(MedTimeDBContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi để debug
                throw new Exception($"Database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}
