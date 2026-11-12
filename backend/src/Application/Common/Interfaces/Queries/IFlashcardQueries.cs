using Application.Common.Models;
using Domain.Flashcards;
using Domain.Sets;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IFlashcardQueries
{
    Task<PagedResult<Flashcard>> GetAllPaged(
        int pageNumber, 
        int? pageSize, 
        SetId? setId,
        CancellationToken cancellationToken);
    Task<Option<Flashcard>> GetById(
        FlashcardId id, 
        CancellationToken cancellationToken);
}