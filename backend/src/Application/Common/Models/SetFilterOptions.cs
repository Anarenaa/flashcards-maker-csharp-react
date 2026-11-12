namespace Application.Common.Models;

public record SetFilterOptions(
    string? SearchTerm = null,
    string? FromLang = null,
    string? ToLang = null,
    bool? IsPublic = true,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);