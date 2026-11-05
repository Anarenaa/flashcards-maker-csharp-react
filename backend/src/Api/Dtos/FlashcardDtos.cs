using Domain.Flashcards;

namespace Api.Dtos;

public record FlashcardDto(
    long Id,
    string Term,
    string Definition,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static FlashcardDto FromDomainModel(Flashcard flashcard)
    {
        return new FlashcardDto(
            flashcard.Id.Value,
            flashcard.Term,
            flashcard.Definition,
            flashcard.CreatedAt,
            flashcard.UpdatedAt);
    }
}

public record CreateFlashcardDto(
    string Term,
    string Definition);

public record UpdateFlashcardDto(
    string Term,
    string Definition);
