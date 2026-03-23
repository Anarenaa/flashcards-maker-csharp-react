using Core.Models;

namespace Repositories.Interfaces
{
    public interface ICollectionRepository : IRepository<Collection>
    {
        Task UpdateCollectionAsync(Collection collection);
        Task<bool> AnySetInCollectionAsync(int collectionId, int setId);
        Task<Set?> LoadSingleSetAsync(Collection collection, int setId);
    }
}
