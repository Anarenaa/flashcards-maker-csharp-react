namespace Core.DTOs
{
    public class FlashcardDTO
    {
        public int? Id { get; set; }
        public required string Term { get; set; }
        public required string Definition { get; set; }
    }
}
