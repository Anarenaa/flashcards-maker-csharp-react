using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class CollectionRepository : Repository<Collection>, ICollectionRepository
    {
        public CollectionRepository(DataContext context) : base(context) { }
        public async Task<bool> AnySetInCollectionAsync(int collectionId, int setId)
        {
            return await _dbSet
                .Where(c => c.Id == collectionId)
                .AnyAsync(c => c.Sets.Any(s => s.Id == setId));
        }
        public async Task<Set?> LoadSingleSetAsync(Collection collection, int setId)
        {
            var query = _context.Entry(collection)
                .Collection(c => c.Sets)
                .Query()
                .Where(s => s.Id == setId);

            await query.LoadAsync();

            return await query.FirstOrDefaultAsync();
        }
        public async Task<int> GetUserCollectionsCount(int userId)
        {
            return await _dbSet.CountAsync(c => c.UserId == userId);
        }
    }
}
