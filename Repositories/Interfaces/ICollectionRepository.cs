using Core.Models;

namespace Repositories.Interfaces
{
    public interface ICollectionRepository : IRepository<Collection>
    {
        Task UpdateCollectionAsync(Collection collection);
    }
}
