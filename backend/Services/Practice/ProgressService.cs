using Core.DTOs.Practice;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;
using Services.Practice.Interfaces;

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

        public async Task ResetSetProgressAsync(int userId, int setId)
        {
            await _unitOfWork.Practice.ResetSetProgressAsync(userId, setId);
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

        public async Task<List<int>> SavePracticeResultsAsync(int userId, List<PracticeResultDTO> results)
        {
            if (results == null || !results.Any()) return new List<int>();

            var incorrectCards = new List<int>();
            var flashcardIds = results.Select(r => r.FlashcardId).Distinct().ToList();

            var currentProgressMap = await _unitOfWork.Practice.GetBatchCardProgressMapAsync(userId, flashcardIds);

            var progressUpdates = new Dictionary<int, float>();

            foreach (var result in results)
            {
                float currentProgress = currentProgressMap.GetValueOrDefault(result.FlashcardId, 0.0f);

                if (result.IsCorrect)
                {
                    float modeLimit = result.ReviewType switch
                    {
                        PracticeActivityType.Review => PracticeActivityLimit.ReviewLimit,
                        PracticeActivityType.Quiz => PracticeActivityLimit.QuizLimit,
                        PracticeActivityType.Matching => PracticeActivityLimit.MatchingLimit,
                        PracticeActivityType.Writing => PracticeActivityLimit.WritingLimit,
                        _ => PracticeActivityLimit.MaxLimit
                    };

                    if (currentProgress < modeLimit)
                    {
                        currentProgress = Math.Min(modeLimit, currentProgress + getProgressIncrease(result.ReviewType));
                    }
                }
                else
                {
                    incorrectCards.Add(result.FlashcardId);
                }

                progressUpdates[result.FlashcardId] = currentProgress;
            }
            await _unitOfWork.Practice.UpsertBatchProgressAsync(userId, progressUpdates);

            return incorrectCards;
        }

        public async Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType, bool isReversed = false)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(flashcardId);
            if (flashcard == null) return false;

            var correctAnswer = isReversed ? flashcard.Term : flashcard.Definition;

            if (string.IsNullOrEmpty(userAnswer) || string.IsNullOrEmpty(correctAnswer))
                return false;

            switch (activityType)
            {
                case PracticeActivityType.Review:
                    return true;
                default:
                    return userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private float getProgressIncrease(PracticeActivityType type) => type switch
        {
            PracticeActivityType.Review => 0.10f,
            PracticeActivityType.Quiz => 0.20f,
            PracticeActivityType.Matching => 0.20f,
            PracticeActivityType.Writing => 0.40f,
            PracticeActivityType.Mixed => 0.10f
        };
    }
}
