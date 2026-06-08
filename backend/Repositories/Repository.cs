using System.Linq.Expressions;
using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class Repository<T> where T : class, IHasCreationDate
    {
        protected readonly DataContext _context;
        protected readonly DbSet<T> _dbSet;
        public Repository(DataContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task<T?> GetByIdAsync(int id, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
