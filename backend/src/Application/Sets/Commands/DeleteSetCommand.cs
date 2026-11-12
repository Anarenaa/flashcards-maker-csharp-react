using Application.Common.Interfaces.Repositories;
using Application.Sets.Exceptions;
using Domain.Sets;
using LanguageExt;
using MediatR;
using Set = Domain.Sets.Set;

namespace Application.Sets.Commands;

public record DeleteSetCommand : IRequest<Either<SetException, Set>>
{
    public required long SetId { get; init; }
}

public class DeleteSetCommandHandler(ISetRepository setRepository)
    : IRequestHandler<DeleteSetCommand, Either<SetException, Set>>
{
    public async Task<Either<SetException, Set>> Handle(
        DeleteSetCommand request,
        CancellationToken cancellationToken)
    {
        var setId = new SetId(request.SetId);
        var set = await setRepository.GetById(setId, cancellationToken);

        return await set.MatchAsync(
            c => DeleteEntity(c, cancellationToken),
            () => new SetNotFoundException(setId)
        );
    }

    private async Task<Either<SetException, Set>> DeleteEntity(
        Set set,
        CancellationToken cancellationToken)
    {
        try
        {
            await setRepository.Delete(set, cancellationToken);
            return set;
        }
        catch (Exception e)
        {
            return new SetUnhandledException(set.Id, e);
        }
    }
}
