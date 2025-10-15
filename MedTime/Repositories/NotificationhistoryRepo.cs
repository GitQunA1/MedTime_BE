using MedTime.Data;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class NotificationhistoryRepo : BaseRepo<Notificationhistory, int>
    {
        private readonly MedTimeDBContext _context;
        public NotificationhistoryRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy notifications của user (có phân trang)
        /// </summary>
        public IQueryable<Notificationhistory> GetByUserIdQuery(int userId)
        {
            return _context.Notificationhistories
                .Where(n => n.Userid == userId)
                .OrderByDescending(n => n.SentAt);
        }

        /// <summary>
        /// Lấy unread notifications của user
        /// </summary>
        public async Task<List<Notificationhistory>> GetUnreadByUserIdAsync(int userId)
        {
            return await _context.Notificationhistories
                .Where(n => n.Userid == userId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notificationhistories
                .FindAsync(notificationId);
            
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Đánh dấu tất cả notifications của user đã đọc
        /// </summary>
        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notificationhistories
                .Where(n => n.Userid == userId && !n.IsRead)
                .ToListAsync();

            var readTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = readTime;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Đếm số notifications chưa đọc
        /// </summary>
        public async Task<int> CountUnreadAsync(int userId)
        {
            return await _context.Notificationhistories
                .CountAsync(n => n.Userid == userId && !n.IsRead);
        }

        /// <summary>
        /// Xóa notifications cũ (> 30 ngày)
        /// </summary>
        public async Task DeleteOldNotificationsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldNotifications = await _context.Notificationhistories
                .Where(n => n.SentAt < cutoffDate)
                .ToListAsync();

            _context.Notificationhistories.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }
    }
}
