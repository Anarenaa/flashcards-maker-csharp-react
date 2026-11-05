using Application.Common.Interfaces.Repositories;
using Application.Flashcards.Exceptions;
using Domain.Flashcards;
using LanguageExt;
using MediatR;

namespace Application.Flashcards.Commands;

public record UpdateFlashcardCommand : IRequest<Either<FlashcardException, Flashcard>>
{
    public required long FlashcardId { get; init; }
    public required string Term { get; init; }
    public required string Definition { get; init; }
}

public class UpdateFlashcardCommandHandler(IFlashcardRepository flashcardRepository)
    : IRequestHandler<UpdateFlashcardCommand, Either<FlashcardException, Flashcard>>
{
    public async Task<Either<FlashcardException, Flashcard>> Handle(
        UpdateFlashcardCommand request,
        CancellationToken cancellationToken)
    {
        var flashcardId = new FlashcardId(request.FlashcardId);
        var flashcard = await flashcardRepository.GetById(flashcardId, cancellationToken);

        return await flashcard.MatchAsync(
            c => UpdateEntity(c, request, cancellationToken),
            () => new FlashcardNotFoundException(flashcardId)
        );
    }

    private async Task<Either<FlashcardException, Flashcard>> UpdateEntity(
        Flashcard flashcard,
        UpdateFlashcardCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            flashcard.UpdateDetails(request.Term, request.Definition);

            return await flashcardRepository.Update(flashcard, cancellationToken);
        }
        catch (Exception e)
        {
            return new FlashcardUnhandledException(flashcard.Id, e);
        }
    }
}
