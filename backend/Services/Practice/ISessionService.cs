using Core.DTOs.Practice;

namespace Services.Practice
{
    public interface ISessionService
    {
        Task<PracticeSessionDTO> GetPracticeSessionAsync(int setId, int userId, PracticeActivityType? requestedMode);
    }
}
