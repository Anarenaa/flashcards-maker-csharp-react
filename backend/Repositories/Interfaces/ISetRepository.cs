using Core.Models;

namespace Repositories.Interfaces
{
    public interface ISetRepository : IRepository<Set>
    {
        Task<int> GetUserSetsCount(int userId);
        Task DeleteUserSets(int userId);
        Task<Dictionary<int, float>> GetOverallProgressForSetsAsync(int userId, List<int> setIds);
        Task<List<int>> FilterSetIdsByProgressAsync(int userId, string progress, bool isMySets = false);
    }
}
