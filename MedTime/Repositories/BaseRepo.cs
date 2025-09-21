using MedTime.Data;
using Microsoft.EntityFrameworkCore;

namespace MedTime.Repositories
{
    public class BaseRepo<T, TKey> where T : class
    {
        private readonly MedTimeDBContext _context;
        public BaseRepo(MedTimeDBContext context)
        {
            _context = context;
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> UpdateAsync(TKey id, T entity)
        {
            var existing = await _context.Set<T>().FindAsync(id);

            if (existing == null)
            {
                return false;
            }

            _context.Entry(existing).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> Delete(TKey id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<T?> GetByIdAsync(TKey id)
        {
            return await _context.Set<T>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id));
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                                .AsNoTracking()
                                .ToListAsync();
        }
    }
}
