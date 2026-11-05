using Domain.Flashcards;

namespace Application.Flashcards.Exceptions;

public abstract class FlashcardException(FlashcardId flashcardId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public FlashcardId FlashcardId { get; } = flashcardId;
}

public class FlashcardNotFoundException(FlashcardId flashcardId)
    : FlashcardException(flashcardId, $"Flashcard not found under id {flashcardId}");

public class FlashcardUnhandledException(FlashcardId flashcardId, Exception? innerException = null)
    : FlashcardException(flashcardId, $"Unhandled flashcard exception under id {flashcardId}", innerException);