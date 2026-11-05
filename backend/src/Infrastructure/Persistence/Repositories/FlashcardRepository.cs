using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Flashcards;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FlashcardRepository(ApplicationDbContext context) : IFlashcardQueries, IFlashcardRepository
{
    public async Task<PagedResult<Flashcard>> GetAllPaged(
        int pageNumber,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Flashcards.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        List<Flashcard> items;

        if (!pageSize.HasValue || pageSize.Value <= 0)
        {
            items = await query
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return new PagedResult<Flashcard>(
                items, 
                totalCount,
                1, 
                totalCount);
        }

        items = await query
            .OrderBy(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .ToListAsync(cancellationToken);

        return new PagedResult<Flashcard>(
            items, 
            totalCount, 
            pageNumber, 
            pageSize.Value);
    }

    public async Task<Option<Flashcard>> GetById(FlashcardId id, CancellationToken cancellationToken)
    {
        var entity = await context.Flashcards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<Flashcard>.None;
    }

    public async Task<Flashcard> Add(Flashcard flashcard, CancellationToken cancellationToken)
    {
        await context.Flashcards.AddAsync(flashcard, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return flashcard;
    }

    public async Task<Flashcard> Update(Flashcard flashcard, CancellationToken cancellationToken)
    {
        context.Flashcards.Update(flashcard);
        await context.SaveChangesAsync(cancellationToken);
        return flashcard;
    }

    public async Task Delete(Flashcard flashcard, CancellationToken cancellationToken)
    {
        context.Flashcards.Remove(flashcard);
        await context.SaveChangesAsync(cancellationToken);
    }
}