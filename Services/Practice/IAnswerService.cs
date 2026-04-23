using Core.DTOs.Practice;

namespace Services.Practice
{
    public interface IAnswerService
    {
        Task<bool> CheckAnswerAsync(int flashcardId, string userAnswer, PracticeActivityType activityType);
    }
}
