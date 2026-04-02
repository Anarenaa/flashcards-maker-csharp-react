using Core.Models;

namespace Repositories.Interfaces
{
    public interface ISetRepository : IRepository<Set>
    {
        Task DeleteUnusedUserSetsAsync(int userId);
        Task UnableSetsWithoutUserInCollections();
        Task<int> GetUserSetsCount(int userId);
    }
}
