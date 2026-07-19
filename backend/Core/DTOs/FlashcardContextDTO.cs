namespace Core.DTOs
{
    public class FlashcardContextDTO
    {
        public int? Id { get; set; }
        public required string Sentence { get; set; }
        public required string Translation { get; set; }
        public bool IsGenerated { get; set; }

    }
}
