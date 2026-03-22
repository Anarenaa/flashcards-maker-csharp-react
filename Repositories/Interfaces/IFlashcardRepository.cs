using Core.Models;

namespace Repositories.Interfaces
{
    public interface IFlashcardRepository : IRepository<Flashcard>
    {
        Task UpdateFlashcardAsync(Flashcard flashcard);
        Task<int> GetCountBySetIdAsync(int setId);
        Task<Dictionary<int, int>> GetCountsBySetIdsAsync(IEnumerable<int> setIds);
    }
}
