using MedTime.Data;
using MedTime.Models.Entities;

namespace MedTime.Repositories
{
    public class UserRepo : BaseRepo<User, int>
    {
        private readonly MedTimeDBContext _context;
        public UserRepo(MedTimeDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
