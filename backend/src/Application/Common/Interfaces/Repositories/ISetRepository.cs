using Application.Common.Models;
using Domain.Sets;
using LanguageExt;
using Set = Domain.Sets.Set;

namespace Application.Common.Interfaces.Repositories;

public interface ISetRepository
{
    Task<Option<Set>> GetById(SetId id, CancellationToken cancellationToken);
    Task<Option<Set>> GetByName(string name, CancellationToken cancellationToken);
    Task<Set> Add(Set set, CancellationToken cancellationToken);
    Task<Set> Update(Set set, CancellationToken cancellationToken);
    Task Delete(Set set, CancellationToken cancellationToken);
}