using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SetRepository : Repository<Set>, ISetRepository
    {
        public SetRepository(DataContext context) : base(context) { }
        public async Task<int> GetUserSetsCount(int userId)
        {
            return await _dbSet.CountAsync(s => s.UserId == userId);
        }
        public async Task DeleteUserSets(int userId)
        {
            var userSets = await _dbSet.Where(s => s.UserId == userId).ToListAsync();
            _dbSet.RemoveRange(userSets);
        }
        public async Task<Dictionary<int, float>> GetOverallProgressForSetsAsync(int userId, List<int> setIds)
        {
            var totalCards = await _context.Flashcards
                .Where(f => setIds.Contains(f.SetId))
                .GroupBy(f => f.SetId)
                .Select(g => new { SetId = g.Key, Total = g.Count() })
                .ToListAsync();

            var progressSums = await _context.CardProgresses
                .Where(p => p.UserId == userId && setIds.Contains(p.Flashcard.SetId))
                .GroupBy(p => p.Flashcard.SetId)
                .Select(g => new { SetId = g.Key, ProgressSum = g.Sum(p => p.Progress) })
                .ToListAsync();

            var progressMap = progressSums.ToDictionary(x => x.SetId, x => x.ProgressSum);

            return totalCards.ToDictionary(
                x => x.SetId,
                x => x.Total > 0
                    ? (progressMap.GetValueOrDefault(x.SetId, 0f) / x.Total)
                    : 0f
            );
        }
    }
}
