using System.Linq.Expressions;
using Core.Models;

namespace Repositories.Interfaces
{
    public interface ISetRepository : IRepository<Set>
    {
        Task<int> GetUserSetsCount(int userId, Expression<Func<Set, bool>>? filter = null);
        Task DeleteUserSets(int userId);
        Task<List<int>> FilterSetIdsByProgressAsync(int userId, string progress, bool isMySets = false);
    }
}
