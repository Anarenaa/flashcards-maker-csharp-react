namespace Core.DTOs.Practice
{
    public class FlashcardPracticeDTO
    {
        public int Id { get; set; }
        public string Term { get; set; }
        public string Definition { get; set; }
        public PracticeActivityType CardActivityType { get; set; } // For Mixed session

        public List<string>? Options { get; set; } // For Quiz session
    }
}
