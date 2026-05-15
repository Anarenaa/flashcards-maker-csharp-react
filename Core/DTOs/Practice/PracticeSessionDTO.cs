namespace Core.DTOs.Practice
{
    public enum PracticeActivityType
    {
        Review = 1,       // Огляд (0-10% прогресу)
        Quiz = 2,         // Вікторина (10-30% прогресу)
        Matching = 3,     // З'єднання пар (30-50% прогресу)
        Writing = 4,      // Письмова відповідь (50-70% прогресу)
        Context = 5,      // AI-контекст у реченні (70-90% прогресу)
        Mixed = 6         // Суміш усіх режимів (90-100% прогресу)
    }
    public static class PracticeActivityLimit
    {
        public const float ReviewLimit = 0.10f;
        public const float QuizLimit = 0.30f;
        public const float MatchingLimit = 0.50f;
        public const float WritingLimit = 0.90f;
        public const float ContextLimit = 0.90f;
        public const float MaxLimit = 1.0f;
    }
    public class PracticeSessionDTO
    {
        public int SetId { get; set; }
        public PracticeActivityType SelectedActivity { get; set; }
        public int BatchSize { get; set; } // порційна видача карток в Matching

        public List<FlashcardPracticeDTO> Flashcards { get; set; } = new();
        public int TotalCards => Flashcards.Count;
    }
}
