namespace Core.Models
{
    public class FlashcardContext
    {
        public int? Id { get; set; }
        public int FlashcardId { get; set; }
        public required Flashcard Flashcard { get; set; }
        public required string Sentence { get; set; }
        public required string Translation { get; set; }
    }
}
