using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class AppointmentRepo : BaseRepo<Appointment, int>
    {
        private readonly MedTimeDBContext _context;
        public AppointmentRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }

    }
}
