namespace Core.Models
{
    public class FlashcardContext : BaseModel
    {
        public int FlashcardId { get; set; }
        public Flashcard Flashcard { get; set; }
        public required string Sentence { get; set; }
        public required string Translation { get; set; }
        public bool IsGenerated { get; set; } = false;
    }
}
