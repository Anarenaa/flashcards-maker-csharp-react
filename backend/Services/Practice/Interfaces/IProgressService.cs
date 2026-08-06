using Core.DTOs.Practice;

namespace Services.Practice.Interfaces
{
    public interface IProgressService
    {
        Task ResetSetProgressAsync(int userId, int setId);
        Task ResetBatchCardProgressAsync(int userId, List<int> flashcardIds);
        Task<float> GetSingleSetProgressAsync(int userId, int setId);
        Task<float> GetOverallProgressForFlascardsBatch(int userId, List<int> flashcardIds);
        Task<UserProgressDTO> GetUserProgressAsync(int userId);
        Task<List<int>> SavePracticeResultsAsync(int userId, List<PracticeResultDTO> results);
        Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType, bool isReversed = false);
    }
}
