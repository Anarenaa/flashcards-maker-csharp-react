using Domain.Sets;

namespace Application.Common.Dtos;

public record SetDto(
    long Id,
    string Name,
    string? Description,
    SetType Type,
    string FromLang,
    string ToLang,
    bool IsPublic,
    bool IsGenerated,
    int FlashcardsCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static SetDto FromDomainModel(Set set, int flashcardsCount = 0)
    {
        return new SetDto(
            set.Id.Value,
            set.Name,
            set.Description,
            set.Type,
            set.FromLang,
            set.ToLang,
            set.IsPublic,
            set.IsGenerated,
            flashcardsCount,
            set.CreatedAt,
            set.UpdatedAt);
    }
}

public record CreateSetDto(
    string Name,
    string? Description,
    SetType Type,
    string FromLang,
    string ToLang,
    bool IsPublic,
    bool IsGenerated);

public record UpdateSetDto(
    string Name,
    string? Description,
    SetType Type,
    string FromLang,
    string ToLang,
    bool IsPublic,
    bool IsGenerated);
