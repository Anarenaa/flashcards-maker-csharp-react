using Core.DTOs;
using Core.DTOs.Practice;

namespace Services.Practice.Interfaces
{
    public interface ISessionService
    {
        Task<PracticeSessionDTO> GetPracticeSessionAsync(
            List<FlashcardDTO> flashcards, 
            int setId, 
            int userId, 
            PracticeActivityType? requestedMode, 
            bool isReversed
        );
    }
}
