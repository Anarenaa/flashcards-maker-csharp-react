using Application.Common.Models;
using Domain.Flashcards;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IFlashcardQueries
{
    Task<PagedResult<Flashcard>> GetAllPaged(
        int pageNumber, 
        int? pageSize, 
        CancellationToken cancellationToken);
    Task<Option<Flashcard>> GetById(
        FlashcardId id, 
        CancellationToken cancellationToken);
}