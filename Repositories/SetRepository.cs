using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SetRepository : Repository<Set>, ISetRepository
    {
        public SetRepository(DataContext context) : base(context) { }
        public async Task UpdateSetAsync(Set set)
        {
            var existingSet = await _dbSet.FindAsync(set.Id);
            if (existingSet is not null)
            {
                existingSet.Name = set.Name;
                existingSet.Description = set.Description;
                existingSet.IsPublic = set.IsPublic;

                existingSet.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
