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
        public async Task ResetBatchCardProgressAsync(int userId, List<int> flashcardIds)
        {
            await _unitOfWork.Practice.ResetBatchCardProgressAsync(userId, flashcardIds);
        }
        public async Task<float> GetSingleSetProgressAsync(int userId, int setId)
        {
            return await _unitOfWork.Practice.GetSingleSetProgressAsync(userId, setId);
        }
        public async Task<float> GetOverallProgressForFlascardsBatch(int userId, List<int> flashcardIds)
        {
            if (flashcardIds == null || flashcardIds.Count == 0) return 0f;

            var progresses = await _unitOfWork.Practice.GetBatchCardProgressMapAsync(userId, flashcardIds);
            if (progresses.Count == 0) return 0f;

            return flashcardIds.Sum(id => progresses.GetValueOrDefault(id, 0f)) / flashcardIds.Count;
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
