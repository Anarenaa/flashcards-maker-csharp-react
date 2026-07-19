using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class FlashcardRepository : Repository<Flashcard>, IFlashcardRepository
    {
        public FlashcardRepository(BaseDataContext context) : base(context) { }
        public async Task<int> GetCountBySetIdAsync(int setId)
        {
            return await _dbSet.CountAsync(f => f.SetId == setId);
        }
        public async Task<Dictionary<int, int>> GetCountsBySetIdsAsync(IEnumerable<int> setIds)
        {
            return await _dbSet
                .Where(f => setIds.Contains(f.SetId))
                .GroupBy(f => f.SetId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
        public async Task<int> GetUserFlashcardsCount(int userId)
        {
            return await _dbSet.CountAsync(f => f.Set.UserId == userId);
        }
    }
}
