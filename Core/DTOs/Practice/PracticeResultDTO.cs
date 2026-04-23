using Core.DTOs.Practice;

namespace Core.DTOs.Practice
{
    public class PracticeResultDTO
    {
        public int FlashcardId { get; set; }
        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; }
        public PracticeActivityType ReviewType { get; set; }
    }

    public class PracticeResultsDTO
    {
        public List<PracticeResultDTO> Results { get; set; } = new();
        public bool IsTrainingMode { get; set; } = false;
    }
}
