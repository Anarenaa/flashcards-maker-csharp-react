using Core.Models;

namespace Repositories.Interfaces
{
    public interface ISetRepository : IRepository<Set>
    {
        public Task UpdateSetAsync(Set set);
    }
}
