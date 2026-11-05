using Domain.Flashcards;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IFlashcardRepository
{
    Task<Option<Flashcard>> GetById(FlashcardId id, CancellationToken cancellationToken);
    Task<Flashcard> Add(Flashcard flashcard, CancellationToken cancellationToken);
    Task<Flashcard> Update(Flashcard flashcard, CancellationToken cancellationToken);
    Task Delete(Flashcard flashcard, CancellationToken cancellationToken);
}