using MedTime.Data;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class PremiumplanRepo : BaseRepo<Premiumplan, int>
    {
        private readonly MedTimeDBContext _context;

        public PremiumplanRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Premiumplan>> GetActivePlansAsync()
        {
            return await _context.Premiumplans
                .AsNoTracking()
                .Where(p => p.Isactive)
                .OrderBy(p => p.Plantype)
                .ToListAsync();
        }

        public async Task<Premiumplan?> GetPlanByIdAsync(int planId)
        {
            return await _context.Premiumplans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Planid == planId && p.Isactive);
        }

        public async Task<Premiumplan?> GetPlanByTypeAsync(PremiumPlanTypeEnum planType)
        {
            return await _context.Premiumplans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Plantype == planType && p.Isactive);
        }
    }
}
