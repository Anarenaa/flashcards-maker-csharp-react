using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class PracticeRepository : IPracticeRepository
    {
        private readonly DataContext _context;
        private DbSet<CardProgress> _dbSet;

        public PracticeRepository(DataContext context)
        {
            _context = context;
            _dbSet = _context.Set<CardProgress>();
        }

        public async Task<List<CardProgress>> GetNewBatchForPracticeAsync(int setId, int userId, int limit)
        {
            return await _dbSet
                .Include(cp => cp.Flashcard)
                .Where(cp => cp.UserId == userId && cp.Flashcard.SetId == setId)
                .Where(cp => cp.Progress < 1.0f) // Тільки те, що не вивчено на 100%
                .OrderBy(cp => cp.Progress)     // Спочатку найменш вивчені
                .ThenBy(cp => cp.NextReview)    // Потім ті, що пора повторити
                .Take(limit)
                .ToListAsync();
        }

        public async Task UpdateProgressAsync(CardProgress progress)
        {
            progress.LastReview = DateTime.UtcNow;

            // інтервальне повторення
            int daysToAdd = (int)(14 * progress.Progress);
            progress.NextReview = DateTime.UtcNow.AddDays(Math.Max(1, daysToAdd));

            _context.CardProgresses.Update(progress);
            await _context.SaveChangesAsync();
        }

        public async Task<CardProgress> GetCardProgressAsync(int userId, int flashcardId)
        {
            return await _dbSet
                .Include(cp => cp.Flashcard)
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.FlashcardId == flashcardId);
        }

        public async Task CreateProgressAsync(CardProgress progress)
        {
            await _context.CardProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CardProgress>> GetAllUserProgressAsync(int userId)
        {
            return await _dbSet
                .Include(cp => cp.Flashcard)
                .Include(cp => cp.Flashcard.Set)
                .Where(cp => cp.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<List<string>> GetDistractorsAsync(int setId, int excludeCardId, int count)
        {
            return await _context.Flashcards
                .Where(c => c.SetId == setId && c.Id != excludeCardId)
                .OrderBy(c => Guid.NewGuid())
                .Take(count)
                .Select(c => c.Definition)
                .ToListAsync();
        }

        public async Task<Dictionary<int, List<string>>> GetBatchDistractorsAsync(int setId, List<int> excludeCardIds, int count)
        {
            var result = new Dictionary<int, List<string>>();
            
            // Отримуємо всі можливі дестрактори для сету
            var allPossibleDistractors = await _context.Flashcards
                .Where(c => c.SetId == setId && !excludeCardIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Definition })
                .ToListAsync();

            foreach (var excludeCardId in excludeCardIds)
            {
                // Беремо випадкові дестрактори, які не є самою карткою
                var distractors = allPossibleDistractors
                    .Where(d => d.Id != excludeCardId)
                    .OrderBy(d => Guid.NewGuid())
                    .Take(count)
                    .Select(d => d.Definition)
                    .ToList();
                    
                result[excludeCardId] = distractors;
            }
            
            return result;
        }
    }
}
