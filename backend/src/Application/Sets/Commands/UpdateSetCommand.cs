using Application.Common.Interfaces.Repositories;
using Application.Sets.Exceptions;
using Domain.Sets;
using LanguageExt;
using MediatR;
using Set = Domain.Sets.Set;
using Unit = LanguageExt.Unit;

namespace Application.Sets.Commands;

public record UpdateSetCommand : IRequest<Either<SetException, Set>>
{
    public required long SetId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SetType Type { get; init; }
    public required string FromLang { get; init; }
    public required string ToLang { get; init; }
    public bool IsPublic { get; init; }
}

public class UpdateSetCommandHandler(ISetRepository setRepository)
    : IRequestHandler<UpdateSetCommand, Either<SetException, Set>>
{
    public async Task<Either<SetException, Set>> Handle(
        UpdateSetCommand request,
        CancellationToken cancellationToken)
    {
        var setId = new SetId(request.SetId);
        var set = await setRepository.GetById(setId, cancellationToken);

        return await set.MatchAsync(
            c => CheckDuplicates(c.Id, request.Name, cancellationToken)
                .BindAsync(_ => UpdateEntity(c, request, cancellationToken)),
            () => new SetNotFoundException(setId)
        );
    }

    private async Task<Either<SetException, Unit>> CheckDuplicates(
        SetId currentSetId,
        string name,
        CancellationToken cancellationToken)
    {
        var existingSet = await setRepository.GetByName(name, cancellationToken);

        return existingSet.Match<Either<SetException, Unit>>(
            c => c.Id == currentSetId ? Unit.Default : new SetAlreadyExistsException(c.Id),
            () => Unit.Default);
    }

    private async Task<Either<SetException, Set>> UpdateEntity(
        Set set,
        UpdateSetCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            set.UpdateDetails(
                request.Name, request.Description, request.Type, 
                request.FromLang, request.ToLang, request.IsPublic);

            return await setRepository.Update(set, cancellationToken);
        }
        catch (Exception e)
        {
            return new SetUnhandledException(set.Id, e);
        }
    }
}
