using Domain.Flashcards;

namespace Domain.Sets;

public class Set
{
    public SetId Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public SetType Type { get; private set; }
    public string FromLang { get; private set; }
    public string ToLang { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsGenerated { get; private set; }
    
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Flashcard>? Flashcards { get; } = null;

    private Set(SetId id, string name, string? description, SetType type, string fromLang, string toLang, bool isPublic, bool isGenerated, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        FromLang = fromLang;
        ToLang = toLang;
        IsPublic = isPublic;
        IsGenerated = isGenerated;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    
    public static Set New(string name, string? description, SetType type, string fromLang, string toLang, bool isPublic = true, bool isGenerated = false) 
            => new(SetId.Empty(), name, description, type, fromLang, toLang, isPublic, isGenerated, DateTime.UtcNow, null);
    
    public void UpdateDetails(string name, string? description, SetType type, string fromLang, string toLang, bool isPublic)
    {
        Name = name;
        Description = description;
        Type = type;
        FromLang = fromLang;
        ToLang = toLang;
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }
}