using Application.Common.Interfaces.Repositories;
using Application.Flashcards.Exceptions;
using Domain.Flashcards;
using LanguageExt;
using MediatR;

namespace Application.Flashcards.Commands;

public record CreateFlashcardCommand : IRequest<Either<FlashcardException, Flashcard>>
{
    public required string Term { get; init; }
    public required string Definition { get; init; }
}

public class CreateFlashcardCommandHandler(IFlashcardRepository flashcardRepository)
    : IRequestHandler<CreateFlashcardCommand, Either<FlashcardException, Flashcard>>
{
    public async Task<Either<FlashcardException, Flashcard>> Handle(
        CreateFlashcardCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var flashcard = Flashcard.New(request.Term, request.Definition);

            return await flashcardRepository.Add(flashcard, cancellationToken);
        }
        catch (Exception e)
        {
            return new FlashcardUnhandledException(FlashcardId.Empty(), e);
        }
    }
}