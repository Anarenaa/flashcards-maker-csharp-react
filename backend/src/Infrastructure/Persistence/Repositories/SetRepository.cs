using Application.Common.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Sets;
using Infrastructure.Persistence.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Set = Domain.Sets.Set;

namespace Infrastructure.Persistence.Repositories;

public class SetRepository(ApplicationDbContext context) : ISetQueries, ISetRepository
{
    public async Task<PagedResult<SetDto>> GetAllPaged(
        int pageNumber, 
        int pageSize, 
        SetFilterOptions? filters, 
        CancellationToken cancellationToken)
    {
        var query = context.Sets.AsNoTracking();
        
        if (filters is not null)
        {
            query = query
                .ApplySearch(filters.SearchTerm)
                .ApplyLanguageFilter(filters.FromLang, filters.ToLang)
                .ApplyPublicityFilter(filters.IsPublic)
                .ApplySorting(filters.SortBy, filters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(s => s.CreatedAt);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SetDto(
                s.Id.Value,
                s.Name,
                s.Description,
                s.Type,
                s.FromLang,
                s.ToLang,
                s.IsPublic,
                s.IsGenerated,
                s.Flashcards!.Count,
                s.CreatedAt,
                s.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
        return new PagedResult<SetDto>(items, totalCount, pageNumber, pageSize);
    }
    async Task<Option<SetDto>> ISetQueries.GetById(SetId id, CancellationToken cancellationToken)
    {
        var set = await context.Sets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (set is null)
        {
            return Option<SetDto>.None;
        }

        var count = await context.Flashcards
            .CountAsync(f => f.SetId == id, cancellationToken);

        return Option<SetDto>.Some(SetDto.FromDomainModel(set, count));
    }

    async Task<Option<Set>> ISetRepository.GetById(SetId id, CancellationToken cancellationToken)
    {
        var entity = await context.Sets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? Option<Set>.None : Option<Set>.Some(entity);
    }

    public async Task<Option<Set>> GetByName(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Sets
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        return entity is null ? Option<Set>.None : Option<Set>.Some(entity);
    }
    
    public async Task<Set> Add(Set set, CancellationToken cancellationToken)
    {
        await context.Sets.AddAsync(set, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return set;
    }

    public async Task<Set> Update(Set set, CancellationToken cancellationToken)
    {
        context.Sets.Update(set);
        await context.SaveChangesAsync(cancellationToken);
        return set;
    }

    public async Task Delete(Set set, CancellationToken cancellationToken)
    {
        context.Sets.Remove(set);
        await context.SaveChangesAsync(cancellationToken);
    }
}
