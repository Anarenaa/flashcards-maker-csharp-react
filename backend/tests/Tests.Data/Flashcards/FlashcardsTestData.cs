using Domain.Flashcards;
using Domain.Sets;

namespace Tests.Data.Flashcards;

public static class FlashcardsTestData
{
    public static Flashcard FirstTestFlashcard(SetId? setId = null) => Flashcard.New("Cat", "Кіт", setId ?? new SetId(1));
    public static Flashcard SecondTestFlashcard(SetId? setId = null) => Flashcard.New("Dog", "Собака", setId ?? new SetId(1));
    public static Flashcard ThirdTestFlashcard(SetId? setId = null) => Flashcard.New("Bird", "Птах", setId ?? new SetId(1));
}
