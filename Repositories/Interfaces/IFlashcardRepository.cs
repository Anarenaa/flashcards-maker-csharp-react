using Core.Models;

namespace Repositories.Interfaces
{
    public interface IFlashcardRepository : IRepository<Flashcard>
    {
        Task<int> GetCountBySetIdAsync(int setId);
        Task<Dictionary<int, int>> GetCountsBySetIdsAsync(IEnumerable<int> setIds);
    }
}
