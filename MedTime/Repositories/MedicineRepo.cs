using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class MedicineRepo : BaseRepo<Medicine, int>
    {
        private readonly MedTimeDBContext _context;
        public MedicineRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
