using Core.DTOs.Practice;

namespace Services.Practice
{
    public interface IProgressService
    {
        Task<SetProgressDTO> GetSetProgressAsync(int setId, int userId);
        Task<UserProgressDTO> GetUserProgressAsync(int userId);
        Task<List<int>> SavePracticeResultsAsync(int userId, PracticeResultsDTO results);
    }
}
