using Core.Models;

namespace Repositories.Interfaces
{
    public interface ISetRepository : IRepository<Set>
    {
        Task<int> GetUserSetsCount(int userId);
        Task DeleteUserSets(int userId);
    }
}
