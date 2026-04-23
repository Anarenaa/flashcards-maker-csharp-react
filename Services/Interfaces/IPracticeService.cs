using Core.DTOs.Practice;

public interface IPracticeService
{
    Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode);
    Task<List<int>> SavePracticeResultsAsync(int userId, PracticeResultsDTO results);
    Task<SetProgressDTO> GetSetProgressAsync(int setId, int userId);
    Task<UserProgressDTO> GetUserProgressAsync(int userId);
    Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType);
}