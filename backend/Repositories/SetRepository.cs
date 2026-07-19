using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SetRepository : Repository<Set>, ISetRepository
    {
        public SetRepository(BaseDataContext context) : base(context) { }
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
        public async Task<List<int>> FilterSetIdsByProgressAsync(int userId, string progress, bool isMySets = false)
        {
            var totalCardsQuery = _context.Flashcards
                .Where(f => isMySets
                    ? f.Set.UserId == userId 
                    : f.Set.IsPublic && f.Set.UserId != userId)
                .GroupBy(f => f.SetId)
                .Select(g => new { SetId = g.Key, TotalCount = g.Count() });

            var userProgressQuery = _context.CardProgresses
                .Where(p => p.UserId == userId)
                .GroupBy(p => p.Flashcard.SetId)
                .Select(g => new
                {
                    SetId = g.Key,
                    StartedCount = g.Count(p => p.Progress >= 0.1f),
                    CompletedCount = g.Count(p => p.Progress >= 1.0f)
                });

            // Склеюємо підзапити через LEFT JOIN
            var query = totalCardsQuery
                .GroupJoin(
                    userProgressQuery,
                    total => total.SetId,
                    up => up.SetId,
                    (total, joined) => new { total, joined }
                )
                .SelectMany(
                    x => x.joined.DefaultIfEmpty(),
                    (x, userProgress) => new
                    {
                        x.total.SetId,
                        x.total.TotalCount,
                        StartedCount = (int?)userProgress.StartedCount ?? 0,
                        CompletedCount = (int?)userProgress.CompletedCount ?? 0
                    }
                );

            var stats = await query.ToListAsync();

            return progress switch
            {
                "notstarted" => stats.Where(x => x.StartedCount == 0).Select(x => x.SetId).ToList(),
                "inprogress" => stats.Where(x => x.StartedCount > 0 && x.CompletedCount < x.TotalCount).Select(x => x.SetId).ToList(),
                "completed" => stats.Where(x => x.CompletedCount == x.TotalCount).Select(x => x.SetId).ToList(),
                _ => new List<int>()
            };
        }
    }
}
