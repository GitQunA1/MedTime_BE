using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class GuardianlinkRepo : BaseRepo<Guardianlink, int>
    {
        private readonly MedTimeDBContext _context;
        public GuardianlinkRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
