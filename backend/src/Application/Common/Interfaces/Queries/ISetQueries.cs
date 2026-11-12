using Application.Common.Dtos;
using Application.Common.Models;
using Domain.Sets;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ISetQueries
{
    Task<PagedResult<SetDto>> GetAllPaged(
        int pageNumber, 
        int pageSize,
        SetFilterOptions? filters,
        CancellationToken cancellationToken);
    Task<Option<SetDto>> GetById(
        SetId id, 
        CancellationToken cancellationToken);
}