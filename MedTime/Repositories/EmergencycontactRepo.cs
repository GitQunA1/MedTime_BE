using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class EmergencycontactRepo : BaseRepo<Emergencycontact, int>
    {
        private readonly MedTimeDBContext _context;
        public EmergencycontactRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
