using Application.Common.Interfaces.Repositories;
using Application.Flashcards.Exceptions;
using Application.Sets.Exceptions;
using Domain.Flashcards;
using Domain.Sets;
using LanguageExt;
using MediatR;
using Set = Domain.Sets.Set;

namespace Application.Flashcards.Commands;

public record CreateFlashcardCommand : IRequest<Either<FlashcardException, Flashcard>>
{
    public required string Term { get; init; }
    public required string Definition { get; init; }
    public required long SetId { get; init; }
}

public class CreateFlashcardCommandHandler(
    IFlashcardRepository flashcardRepository,
    ISetRepository setRepository)
    : IRequestHandler<CreateFlashcardCommand, Either<FlashcardException, Flashcard>>
{
    public async Task<Either<FlashcardException, Flashcard>> Handle(
        CreateFlashcardCommand request,
        CancellationToken cancellationToken)
    {
        var setId = new SetId(request.SetId);
        var setOption = await setRepository.GetById(setId, cancellationToken);

        return await setOption.MatchAsync(
            _ => CreateEntity(request, setId, cancellationToken),
            () => new FlashcardSetNotFoundException(setId)
        );
    }

    private async Task<Either<FlashcardException, Flashcard>> CreateEntity(
        CreateFlashcardCommand request,
        SetId setId,
        CancellationToken cancellationToken)
    {
        try
        {
            var flashcard = Flashcard.New(request.Term, request.Definition, setId);
            return await flashcardRepository.Add(flashcard, cancellationToken);
        }
        catch (Exception e)
        {
            return new FlashcardUnhandledException(FlashcardId.Empty(), e);
        }
    }
}