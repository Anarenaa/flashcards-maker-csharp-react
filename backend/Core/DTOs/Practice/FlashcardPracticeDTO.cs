namespace Core.DTOs.Practice
{
    public class FlashcardPracticeDTO
    {
        public int Id { get; set; }
        public required string Term { get; set; }
        public required string Definition { get; set; }
        public string? FromLang { get; set; }
        public string? ToLang { get; set; }
        public PracticeActivityType CardActivityType { get; set; } // For Mixed session

        public List<string>? Options { get; set; } // For Quiz session
    }
}
