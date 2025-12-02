using MedTime.Data;
using MedTime.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class GuardianlinkRepo : BaseRepo<Guardianlink, int>
    {
        private readonly MedTimeDBContext _context;
        public GuardianlinkRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        // Override GetAllQuery để include Guardian và Patient navigation properties
        public override IQueryable<Guardianlink> GetAllQuery()
        {
            return _context.Guardianlinks
                .Include(g => g.Guardian)
                .Include(g => g.Patient)
                .AsNoTracking();
        }

        // Override GetAllAsync để include Guardian và Patient navigation properties
        public override async Task<List<Guardianlink>> GetAllAsync()
        {
            return await _context.Guardianlinks
                .Include(g => g.Guardian)
                .Include(g => g.Patient)
                .AsNoTracking()
                .ToListAsync();
        }

        // Guardianlink có composite key nên cần override method này
        public async Task<Guardianlink?> GetByIdAsync(int guardianId, int patientId)
        {
            return await _context.Guardianlinks
                .Include(g => g.Guardian)
                .Include(g => g.Patient)
                .FirstOrDefaultAsync(g => g.Guardianid == guardianId && g.Patientid == patientId);
        }

        /// <summary>
        /// Xóa guardian link bằng composite key (guardianId, patientId)
        /// </summary>
        public async Task<bool> DeleteByCompositeKeyAsync(int guardianId, int patientId)
        {
            var entity = await _context.Guardianlinks
                .FirstOrDefaultAsync(g => g.Guardianid == guardianId && g.Patientid == patientId);
            
            if (entity == null) return false;

            _context.Guardianlinks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
