namespace Core.DTOs.Practice
{
    public class PracticeResultDTO
    {
        public int FlashcardId { get; set; }
        public bool IsCorrect { get; set; }
        public PracticeActivityType ReviewType { get; set; }
    }
}
