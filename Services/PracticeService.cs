using Core.DTOs.Practice;
using Services.Practice;

namespace Services
{
    public class PracticeService : IPracticeService
    {
        private readonly ISessionService _sessionService;
        private readonly IProgressService _progressService;
        private readonly IAnswerService _answerService;

        public PracticeService(ISessionService sessionService, IProgressService progressService, IAnswerService answerService)
        {
            _sessionService = sessionService;
            _progressService = progressService;
            _answerService = answerService;
        }

        public async Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode)
        {
            return await _sessionService.GetPracticeSessionAsync(setId, userId, requestedMode);
        }

        public async Task<List<int>> SavePracticeResultsAsync(int userId, PracticeResultsDTO results)
        {
            return await _progressService.SavePracticeResultsAsync(userId, results);
        }

        public async Task<SetProgressDTO> GetSetProgressAsync(int setId, int userId)
        {
            return await _progressService.GetSetProgressAsync(setId, userId);
        }

        public async Task<UserProgressDTO> GetUserProgressAsync(int userId)
        {
            return await _progressService.GetUserProgressAsync(userId);
        }

        public async Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType)
        {
            return await _answerService.CheckAnswerAsync(flashcardId, userAnswer, activityType);
        }
    }
}
