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
