using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class IntakelogRepo : BaseRepo<Intakelog, int>
    {
        private readonly MedTimeDBContext _context;
        public IntakelogRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
