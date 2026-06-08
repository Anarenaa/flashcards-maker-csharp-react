namespace Core.DTOs.Practice
{
    public class FlashcardPracticeDTO
    {
        public int Id { get; set; }
        public string Term { get; set; }
        public string Definition { get; set; }
        public PracticeActivityType CardType { get; set; } // Для Mixed

        public List<string>? Distractors { get; set; } // Тільки для Quiz
    }
}
