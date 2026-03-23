namespace Core.DTOs
{
    public class SetDetailDTO : SetDTO
    {
        public List<FlashcardDTO> Flashcards { get; set; } = new();
        public List<CategoryDTO> Categories { get; set; } = new();
    }
}
