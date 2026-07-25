using Core.DTOs;
using Core.DTOs.Practice;
using Services.Practice.Interfaces;

namespace Services.Practice
{
    public class PracticeService : IPracticeService
    {
        private readonly ISessionService _sessionService;
        private readonly IProgressService _progressService;

        public PracticeService(ISessionService sessionService, IProgressService progressService)
        {
            _sessionService = sessionService;
            _progressService = progressService;
        }

        public async Task<PracticeSessionDTO> GetPracticeSessionAsync(
            List<FlashcardDTO> flashcards,
            int setId,
            int userId,
            PracticeActivityType? requestedMode,
            bool isReversed)
        {
            return await _sessionService.GetPracticeSessionAsync(flashcards, setId, userId, requestedMode, isReversed);
        }

        public async Task<UserProgressDTO> GetUserProgressAsync(int userId)
        {
            return await _progressService.GetUserProgressAsync(userId);
        }

        public async Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType, bool isReversed)
        {
            return await _progressService.CheckAnswerAsync(flashcardId, userAnswer, activityType, isReversed);
        }
        public async Task<List<int>> SavePracticeResultsAsync(int userId, List<PracticeResultDTO> results)
        {
            return await _progressService.SavePracticeResultsAsync(userId, results);
        }
    }
}
