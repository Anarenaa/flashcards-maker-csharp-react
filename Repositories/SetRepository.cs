using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SetRepository : Repository<Set>, ISetRepository
    {
        public SetRepository(DataContext context) : base(context) { }
        public async Task DeleteUnusedUserSetsAsync(int userId)
        {
            var setsWithoutCollection = await _dbSet
                .Where(s => s.UserId == userId && !s.Collections.Any())
                .ToListAsync();

            if (setsWithoutCollection.Any())
            {
                _dbSet.RemoveRange(setsWithoutCollection);
            }
        }
        public async Task UnableSetsWithoutUserInCollections()
        {
            var setsWithoutUser = await _dbSet
                .Where(s => s.UserId == null && s.Collections.Any())
                .ToListAsync();
            foreach (var set in setsWithoutUser)
            {
                set.IsAccessible = false;
            }
        }
    }
}
