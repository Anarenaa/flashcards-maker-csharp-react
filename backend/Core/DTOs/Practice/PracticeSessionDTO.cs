namespace Core.DTOs.Practice
{
    public class PracticeSessionDTO
    {
        public PracticeActivityType SelectedActivity { get; set; }

        public List<FlashcardPracticeDTO> Flashcards { get; set; } = new();
    }
}
