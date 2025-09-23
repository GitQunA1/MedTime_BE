using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class PrescriptionscheduleRepo : BaseRepo<Prescriptionschedule, int>
    {
        private readonly MedTimeDBContext _context;
        public PrescriptionscheduleRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
