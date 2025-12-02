using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class CalllogRepo : BaseRepo<Calllog, int>
    {
        private readonly MedTimeDBContext _context;
        public CalllogRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
