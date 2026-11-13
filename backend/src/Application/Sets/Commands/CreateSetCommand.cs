using Application.Common.Interfaces.Repositories;
using Application.Sets.Exceptions;
using Domain.Sets;
using LanguageExt;
using MediatR;
using Set = Domain.Sets.Set;

namespace Application.Sets.Commands;

public record CreateSetCommand : IRequest<Either<SetException, Set>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SetType Type { get; init; }
    public required string FromLang { get; init; }
    public required string ToLang { get; init; }
    public bool IsPublic { get; init; }
    public bool IsGenerated { get; init; }
}

public class CreateSetCommandHandler(ISetRepository setRepository)
    : IRequestHandler<CreateSetCommand, Either<SetException, Set>>
{
    public async Task<Either<SetException, Set>> Handle(
        CreateSetCommand request,
        CancellationToken cancellationToken)
    {
        var existingSet = await setRepository.GetByName(request.Name, cancellationToken);

        return await existingSet.MatchAsync(
            c => new SetAlreadyExistsException(c.Id),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<SetException, Set>> CreateEntity(
        CreateSetCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var set = Set.New(
                request.Name, request.Description, request.Type, 
                request.FromLang, request.ToLang, request.IsPublic, request.IsGenerated);

            return await setRepository.Add(set, cancellationToken);
        }
        catch (Exception e)
        {
            return new SetUnhandledException(SetId.Empty(), e);
        }
    }
}
