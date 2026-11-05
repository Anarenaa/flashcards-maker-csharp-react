namespace Domain.Flashcards;

public record FlashcardId(long Value)
{
    public static FlashcardId Empty()
    {
        return new FlashcardId(0);
    }

    public static FlashcardId New()
    {
        return new FlashcardId(0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}