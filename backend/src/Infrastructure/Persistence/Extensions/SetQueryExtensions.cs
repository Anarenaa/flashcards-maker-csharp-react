using Domain.Sets;

namespace Infrastructure.Persistence.Extensions;

public static class SetQueryExtensions
{
    public static IQueryable<Set> ApplySearch(this IQueryable<Set> query, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return query;

        return query.Where(s => 
            s.Name.ToLower().Contains(term.ToLower()) || 
            (s.Description != null && s.Description.ToLower().Contains(term.ToLower())));
    }

    public static IQueryable<Set> ApplyLanguageFilter(this IQueryable<Set> query, string? fromLang, string? toLang)
    {
        if (!string.IsNullOrWhiteSpace(fromLang))
            query = query.Where(s => s.FromLang == fromLang);

        if (!string.IsNullOrWhiteSpace(toLang))
            query = query.Where(s => s.ToLang == toLang);

        return query;
    }
    public static IQueryable<Set> ApplyPublicityFilter(this IQueryable<Set> query, bool? isPublic)
    {
        if (isPublic is not null)
            query = query.Where(s => s.IsPublic == isPublic);

        return query;
    }

    public static IQueryable<Set> ApplySorting(this IQueryable<Set> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("name", true) => query.OrderByDescending(s => s.Name),
            ("name", false) => query.OrderBy(s => s.Name),
            ("createdat", true) => query.OrderByDescending(s => s.CreatedAt),
            ("createdat", false) => query.OrderBy(s => s.CreatedAt),
            ("updatedat", true) => query.OrderByDescending(s => s.UpdatedAt),
            ("updatedat", false) => query.OrderBy(s => s.UpdatedAt),
            (_, false) => query.OrderBy(s => s.Id),
            _ => query.OrderByDescending(s => s.Id)
        };
    }
}