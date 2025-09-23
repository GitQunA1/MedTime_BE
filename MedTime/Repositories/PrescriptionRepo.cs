using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class PrescriptionRepo : BaseRepo<Prescription, int>
    {
        private readonly MedTimeDBContext _context;
        public PrescriptionRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
