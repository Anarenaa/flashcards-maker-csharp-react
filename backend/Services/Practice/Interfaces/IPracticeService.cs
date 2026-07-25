using Core.DTOs;
using Core.DTOs.Practice;

public interface IPracticeService
{
    Task<PracticeSessionDTO> GetPracticeSessionAsync(List<FlashcardDTO> flashcards, int setId, int userId, PracticeActivityType? requestedMode, bool isReversed);
    Task<float> GetSingleSetProgressAsync(int userId, int setId);
    Task<UserProgressDTO> GetUserProgressAsync(int userId);
    Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType, bool isReversed);
    Task<List<int>> SavePracticeResultsAsync(int userId, List<PracticeResultDTO> results);
}