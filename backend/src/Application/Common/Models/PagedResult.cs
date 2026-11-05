namespace Application.Common.Models;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => PageSize > 0 
        ? (int)Math.Ceiling(TotalCount / (double)PageSize) 
        : 1;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    // Transforms Items (e.g., from Flashcard to FlashcardDto) while keeping pagination metadata
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapFunc) =>
        new(Items.Select(mapFunc).ToList(), TotalCount, PageNumber, PageSize);
}