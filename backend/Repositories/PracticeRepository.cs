using Core.Context;
using Core.DTOs;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class PracticeRepository : IPracticeRepository
    {
        private readonly BaseDataContext _context;
        private DbSet<CardProgress> _dbSet;

        public PracticeRepository(BaseDataContext context)
        {
            _context = context;
            _dbSet = _context.Set<CardProgress>();
        }

        // =====================================================================
        // PROGRESS RETRIEVAL METHODS
        // Methods responsible for fetching tracking data and user progress statistics.
        // =====================================================================
        public async Task<Dictionary<int, float>> GetBatchCardProgressMapAsync(int userId, List<int> flashcardIds)
        {
            return await _context.CardProgresses
                .Where(cp => cp.UserId == userId && flashcardIds.Contains(cp.FlashcardId))
                .ToDictionaryAsync(cp => cp.FlashcardId, cp => cp.Progress);
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

        public async Task<List<CardProgress>> GetAllUserProgressAsync(int userId)
        {
            return await _dbSet
                .Include(cp => cp.Flashcard)
                .Include(cp => cp.Flashcard.Set)
                .Where(cp => cp.UserId == userId)
                .ToListAsync();
        }

        // =====================================================================
        // PROGRESS MUTATION METHODS
        // Methods handling creation, updates, batch loading, and removal of progress.
        // =====================================================================

        public async Task<List<CardProgress>> GetNewBatchForPracticeAsync(List<FlashcardDTO> flashcards, int userId)
        {
            var cardIds = flashcards.Select(f => f.Id).ToList();

            var existingProgress = await _dbSet
                .Where(cp => cp.UserId == userId && cardIds.Contains(cp.FlashcardId))
                .ToListAsync();

            var missingCards = flashcards.Where(f => !existingProgress.Any(cp => cp.FlashcardId == f.Id)).ToList();

            if (missingCards.Any())
            {
                var newEntries = missingCards.Select(f => new CardProgress
                {
                    FlashcardId = (int)f.Id,
                    UserId = userId,
                    Progress = 0.0f,
                    LastReview = DateTime.MinValue,
                    NextReview = DateTime.UtcNow
                }).ToList();

                await _dbSet.AddRangeAsync(newEntries);
                await _context.SaveChangesAsync();

                existingProgress.AddRange(newEntries);
            }

            return existingProgress
                .OrderBy(cp => cp.Progress)
                .ThenBy(cp => cp.NextReview)
                .ToList();
        }

        // Update + Insert for CardProgress
        public async Task UpsertProgressAsync(int userId, int flashcardId, float newProgress)
        {
            var progress = await _dbSet.FirstOrDefaultAsync(cp => cp.UserId == userId && cp.FlashcardId == flashcardId);

            if (progress == null)
            {
                progress = new CardProgress
                {
                    UserId = userId,
                    FlashcardId = flashcardId,
                    Progress = newProgress,
                    LastReview = DateTime.UtcNow,
                    NextReview = DateTime.UtcNow.AddDays(14 * newProgress)
                };
                await _dbSet.AddAsync(progress);
            }
            else
            {
                progress.Progress = newProgress;
                progress.LastReview = DateTime.UtcNow;
                int daysToAdd = (int)(14 * progress.Progress);
                progress.NextReview = DateTime.UtcNow.AddDays(Math.Max(1, daysToAdd));
                _dbSet.Update(progress);
            }

            await _context.SaveChangesAsync();
        }
        public async Task UpsertBatchProgressAsync(int userId, Dictionary<int, float> progressUpdates)
        {
            if (!progressUpdates.Any()) return;

            var cardIds = progressUpdates.Keys.ToList();

            // one query 
            var existingProgresses = await _context.CardProgresses
                .Where(cp => cp.UserId == userId && cardIds.Contains(cp.FlashcardId))
                .ToListAsync();

            foreach (var pair in progressUpdates)
            {
                int flashcardId = pair.Key;
                float newProgress = pair.Value;

                var existing = existingProgresses.FirstOrDefault(cp => cp.FlashcardId == flashcardId);
                if (existing != null)
                {
                    existing.Progress = newProgress;
                    existing.LastReview = DateTime.UtcNow;
                }
                else
                {
                    _context.CardProgresses.Add(new CardProgress
                    {
                        UserId = userId,
                        FlashcardId = flashcardId,
                        Progress = newProgress,
                        LastReview = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task CreateProgressAsync(CardProgress progress)
        {
            await _context.CardProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }
        public async Task ResetSetProgressAsync(int userId, int setId)
        {
            // ExecuteUpdateAsync updates the database directly with a single SQL query, 
            // so it doesn't load entities into memory or require SaveChangesAsync()
            await _context.CardProgresses
                .Where(cp => cp.UserId == userId && cp.Flashcard.SetId == setId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Progress, 0.0f));
        }

        // =====================================================================
        // PRACTICE ASSETS & GENERATION METHODS
        // Methods used to generate distractors and support quiz or practice logic.
        // =====================================================================

        public async Task<Dictionary<int, List<string>>> GetBatchDistractorsAsync(int setId, List<int> excludeCardIds, int count, bool isReversed)
        {
            var result = new Dictionary<int, List<string>>();

            if (isReversed)
            {
                // all distractors will be terms
                var allTerms = await _context.Flashcards
                    .Where(c => c.SetId == setId && !excludeCardIds.Contains(c.Id))
                    .Select(c => new { c.Id, Value = c.Term })
                    .ToListAsync();

                foreach (var excludeCardId in excludeCardIds)
                {
                    result[excludeCardId] = allTerms
                        .Where(d => d.Id != excludeCardId)
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(count)
                        .Select(d => d.Value)
                        .ToList();
                }
            }
            else
            {
                // all distractors will be definitions
                var allDefinitions = await _context.Flashcards
                    .Where(c => c.SetId == setId && !excludeCardIds.Contains(c.Id))
                    .Select(c => new { c.Id, Value = c.Definition })
                    .ToListAsync();

                foreach (var excludeCardId in excludeCardIds)
                {
                    result[excludeCardId] = allDefinitions
                        .Where(d => d.Id != excludeCardId)
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(count)
                        .Select(d => d.Value)
                        .ToList();
                }
            }

            return result;
        }
    }
}