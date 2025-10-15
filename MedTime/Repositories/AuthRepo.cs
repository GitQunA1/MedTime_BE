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

        public async Task<bool> IsUniqueCodeAvailableAsync(string uniqueCode)
        {
            return !await _context.Users.AnyAsync(u => u.Uniquecode == uniqueCode);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Userid == userId);
        }

        public async Task<bool> UpdatePasswordAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}
