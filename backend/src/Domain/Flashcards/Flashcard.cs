using Domain.Sets;

namespace Domain.Flashcards;

public class Flashcard
{
    public FlashcardId Id { get; private set; }
    public string Term { get; private set; }
    public string Definition { get; private set; }
    
    public SetId SetId { get; private set; }
    public Set? Set { get; }
    
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }
    
    private Flashcard(FlashcardId id, string term, string definition, SetId setId, DateTime createdAt, DateTime? updatedAt = null)
    {
        Id = id;
        Term = term;
        Definition = definition;
        SetId = setId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Flashcard New(string term, string definition, SetId setId) 
        => new(FlashcardId.Empty(), term, definition, setId, DateTime.UtcNow);

    public void UpdateDetails(string term, string definition)
    {
        Term = term;
        Definition = definition;
        UpdatedAt = DateTime.UtcNow;
    }
}