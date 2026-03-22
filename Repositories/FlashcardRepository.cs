using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class FlashcardRepository : Repository<Flashcard>, IFlashcardRepository
    {
        public FlashcardRepository(DataContext context) : base(context) { }
        public async Task UpdateFlashcardAsync(Flashcard flashcard)
        {
            var existingFlashcard = await _dbSet.FindAsync(flashcard.Id);
            if (existingFlashcard is not null)
            {
                existingFlashcard.Term = flashcard.Term;
                existingFlashcard.Definition = flashcard.Definition;
                existingFlashcard.UpdatedAt = DateTime.UtcNow;
            }
        }
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
    }
}
