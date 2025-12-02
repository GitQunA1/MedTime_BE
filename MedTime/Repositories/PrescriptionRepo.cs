using MedTime.Data;
using MedTime.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class PrescriptionRepo : BaseRepo<Prescription, int>
    {
        private readonly MedTimeDBContext _context;
        public PrescriptionRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        public Task<int> CountByUserAsync(int userId)
        {
            return _context.Prescriptions
                .Where(p => p.Userid == userId)
                .AsNoTracking()
                .CountAsync();
        }
    }
}
