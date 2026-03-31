namespace Core.DTOs.Practice
{
    public enum PracticeActivityType
    {
        Quiz = 1,        // Вікторина (0-20% прогресу)
        Matching = 2,    // З'єднання пар (20-40% прогресу)
        Writing = 3,     // Письмова відповідь (40-60% прогресу)
        Context = 4,     // AI-контекст у реченні (60-80% прогресу)
        Mixed = 5        // Суміш усіх режимів (80-100% прогресу)
    }
    public class PracticeSessionDTO
    {
        public int SetId { get; set; }
        public string SetTitle { get; set; }
        public PracticeActivityType SelectedActivity { get; set; }
        public int BatchSize { get; set; } // порційна видача карток в Matching

        public List<FlashcardPracticeDTO> Flashcards { get; set; } = new();
        public int TotalCards => Flashcards.Count;
    }
}
