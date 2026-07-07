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
        public async Task<PagedResult<T>> GetAllPagedAsync(
            int page = 0,
            int pageSize = 20,
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

            int totalCount = await query.CountAsync();

            query = query.OrderByDescending(x => x.CreatedAt);

            List<T> items;

            if (page == 0)
            {
                items = await query.ToListAsync();
                pageSize = totalCount;
                page = 1;
            }
            else
            {
                int skipAmount = (page - 1) * pageSize;
                items = await query.Skip(skipAmount).Take(pageSize).ToListAsync();
            }

            return new PagedResult<T>(items, totalCount, page, pageSize);
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
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PagedResult(IEnumerable<T> items, int totalItems, int page, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            CurrentPage = page;
            PageSize = pageSize;
        }
    }
}
