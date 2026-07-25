using Core.DTOs;
using Core.Models;

namespace Repositories.Interfaces
{
    public interface IPracticeRepository
    {
        // Progress Retrieval
        Task<Dictionary<int, float>> GetBatchCardProgressMapAsync(int userId, List<int> flashcardIds);
        Task<float> GetSingleSetProgressAsync(int userId, int setId);
        Task<Dictionary<int, float>> GetOverallProgressForSetsAsync(int userId, List<int> setIds);
        Task<List<CardProgress>> GetAllUserProgressAsync(int userId);

        // Progress Mutation
        Task<List<CardProgress>> GetNewBatchForPracticeAsync(List<FlashcardDTO> flashcards, int userId);
        Task UpsertProgressAsync(int userId, int flashcardId, float newProgress);
        Task UpsertBatchProgressAsync(int userId, Dictionary<int, float> progressUpdates);
        Task CreateProgressAsync(CardProgress progress);
        Task ResetSetProgressAsync(int userId, int setId);

        // Practice Assets
        Task<Dictionary<int, List<string>>> GetBatchDistractorsAsync(int setId, List<int> excludeCardIds, int count, bool isReversed);
    }
}