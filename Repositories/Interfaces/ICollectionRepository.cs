using Core.Models;

namespace Repositories.Interfaces
{
    public interface ICollectionRepository : IRepository<Collection>
    {
        Task<bool> AnySetInCollectionAsync(int collectionId, int setId);
        Task<Set?> LoadSingleSetAsync(Collection collection, int setId);
        Task<int> GetUserCollectionsCount(int userId);
    }
}
