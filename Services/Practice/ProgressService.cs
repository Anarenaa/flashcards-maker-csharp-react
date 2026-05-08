using Core.DTOs.Practice;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;

namespace Services.Practice
{
    public class ProgressService : IProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public ProgressService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<SetProgressDTO> GetSetProgressAsync(int setId, int userId)
        {
            var allFlashcards = await _unitOfWork.Flashcards.GetAllAsync(f => f.SetId == setId);
            if (!allFlashcards.Any())
                return new SetProgressDTO { SetId = setId, TotalCards = 0, OverallProgress = 0f };

            var existingProgress = await _unitOfWork.Practice.GetSetProgressAsync(userId, setId);

            var progressMap = existingProgress.ToDictionary(p => p.FlashcardId);

            var cardProgresses = new List<CardProgress>();

            foreach (var flashcard in allFlashcards)
            {
                if (progressMap.TryGetValue(flashcard.Id, out var progress))
                {
                    cardProgresses.Add(progress);
                }
                else
                {
                    cardProgresses.Add(new CardProgress
                    {
                        FlashcardId = flashcard.Id,
                        UserId = userId,
                        Progress = 0.0f
                    });
                }
            }

            // Розраховуємо загальний прогрес
            var overallProgress = cardProgresses.Average(cp => cp.Progress);

            // Розраховуємо статистику
            var masteredCards = cardProgresses.Count(cp => cp.Progress >= 1.0f);
            var inProgressCards = cardProgresses.Count(cp => cp.Progress >= 0.1f && cp.Progress < 1.0f);
            var notStartedCards = cardProgresses.Count(cp => cp.Progress < 0.1f);

            return new SetProgressDTO
            {
                SetId = setId,
                TotalCards = allFlashcards.Count(),
                OverallProgress = overallProgress,
                MasteredCards = masteredCards,
                InProgressCards = inProgressCards,
                NotStartedCards = notStartedCards
            };
        }

        public async Task<UserProgressDTO> GetUserProgressAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new UserProgressDTO { UserId = userId };

            // Отримуємо всі прогреси користувача (з усіх сетів, які він практикував)
            var allCardProgresses = await _unitOfWork.Practice.GetAllUserProgressAsync(userId);
            if (!allCardProgresses.Any())
                return new UserProgressDTO { UserId = userId };

            // Групуємо по сетах
            var setProgresses = new List<SetProgressSummary>();
            var setGroups = allCardProgresses.GroupBy(cp => cp.Flashcard.SetId);

            foreach (var setGroup in setGroups)
            {
                var setCardProgresses = setGroup.ToList();
                var setOverallProgress = setCardProgresses.Average(cp => cp.Progress);
                var setMasteredCards = setCardProgresses.Count(cp => cp.Progress >= 1.0f);

                setProgresses.Add(new SetProgressSummary
                {
                    SetId = setGroup.Key,
                    Progress = setOverallProgress,
                    MasteredCards = setMasteredCards
                });
            }

            // Розраховуємо загальну статистику по всіх пройдених картах
            var practicedCards = allCardProgresses.Count;
            var overallProgress = allCardProgresses.Average(cp => cp.Progress);
            var masteredCards = allCardProgresses.Count(cp => cp.Progress >= 1.0f);
            var inProgressCards = allCardProgresses.Count(cp => cp.Progress >= 0.1f && cp.Progress < 1.0f);
            var notStartedCards = allCardProgresses.Count(cp => cp.Progress < 0.1f);

            // Отримуємо статистику активності
            var lastActivity = allCardProgresses.Any() ? allCardProgresses.Max(cp => cp.LastReview) : DateTime.MinValue;
            var totalPracticeSessions = allCardProgresses.Count(cp => cp.LastReview > DateTime.MinValue);

            return new UserProgressDTO
            {
                UserId = userId,
                SetProgresses = setProgresses,
                PracticedSets = setProgresses.Count,
                CompletedSets = setProgresses.Count(sp => sp.IsCompleted),
                PracticedCards = practicedCards,
                MasteredCards = masteredCards,
                InProgressCards = inProgressCards,
                NotStartedCards = notStartedCards,
                OverallProgress = overallProgress,
                LastActivity = lastActivity,
                TotalPracticeSessions = totalPracticeSessions,
            };
        }

        public async Task<List<int>> SavePracticeResultsAsync(int userId, PracticeResultsDTO results)
        {
            var incorrectCards = new List<int>();

            foreach (var result in results.Results)
            {
                var progress = await _unitOfWork.Practice.GetCardProgressAsync(userId, result.FlashcardId);
                
                if (progress == null)
                {
                    // Створюємо новий прогрес якщо не існує
                    progress = new CardProgress
                    {
                        FlashcardId = result.FlashcardId,
                        UserId = userId,
                        Progress = 0.0f,
                        LastReview = DateTime.UtcNow
                    };
                    
                    await _unitOfWork.Practice.CreateProgressAsync(progress);
                }

                if (result.IsCorrect)
                {
                    float modeLimit = result.ReviewType switch
                    {
                        PracticeActivityType.Review => PracticeActivityLimit.ReviewLimit,
                        PracticeActivityType.Quiz => PracticeActivityLimit.QuizLimit,
                        PracticeActivityType.Matching => PracticeActivityLimit.MatchingLimit,
                        PracticeActivityType.Writing => PracticeActivityLimit.WritingLimit,
                        PracticeActivityType.Context => PracticeActivityLimit.ContextLimit,
                        _ => PracticeActivityLimit.MaxLimit
                    };

                    // Збільшуємо прогрес
                    if (progress.Progress < modeLimit)
                    {
                        progress.Progress = Math.Min(modeLimit, progress.Progress + GetProgressIncrease(result.ReviewType));
                    }
                }
                else
                {
                    incorrectCards.Add(result.FlashcardId);
                }
                
                await _unitOfWork.Practice.UpdateProgressAsync(progress);
            }

            return incorrectCards;
        }

        private float GetProgressIncrease(PracticeActivityType type) => type switch
        {
            PracticeActivityType.Review => 0.10f,
            PracticeActivityType.Quiz => 0.20f,
            PracticeActivityType.Matching => 0.20f,
            PracticeActivityType.Writing => 0.20f,
            PracticeActivityType.Context => 0.20f,
            PracticeActivityType.Mixed => 0.10f
        };
    }
}
