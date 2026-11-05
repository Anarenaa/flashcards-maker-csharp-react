using Domain.Flashcards;

namespace Tests.Data.Flashcards;

public static class FlashcardsTestData
{
    public static Flashcard FirstTestFlashcard() => Flashcard.New("Cat", "Кіт");
    public static Flashcard SecondTestFlashcard() => Flashcard.New("Dog", "Собака");
    public static Flashcard ThirdTestFlashcard() => Flashcard.New("Bird", "Птах");
}
