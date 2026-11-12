using Domain.Flashcards;

namespace Application.Common.Dtos;

public record FlashcardDto(
    long Id,
    string Term,
    string Definition,
    long SetId,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static FlashcardDto FromDomainModel(Flashcard flashcard)
    {
        return new FlashcardDto(
            flashcard.Id.Value,
            flashcard.Term,
            flashcard.Definition,
            flashcard.SetId.Value,
            flashcard.CreatedAt,
            flashcard.UpdatedAt);
    }
}

public record CreateFlashcardDto(
    string Term,
    string Definition,
    long SetId);

public record UpdateFlashcardDto(
    string Term,
    string Definition);
