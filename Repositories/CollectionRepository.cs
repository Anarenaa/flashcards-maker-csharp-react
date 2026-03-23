using Core.Context;
using Core.Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CollectionRepository : Repository<Collection>, ICollectionRepository
    {
        public CollectionRepository(DataContext context) : base(context) { }
        public async Task UpdateCollectionAsync(Collection collection)
        {
            var existingCollection = await _dbSet.FindAsync(collection.Id);
            if (existingCollection is not null)
            {
                existingCollection.Name = collection.Name;
                existingCollection.Description = collection.Description;
                existingCollection.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
