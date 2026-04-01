using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Set : BaseModel
    {

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = true;
        public bool IsAccessible { get; set; } = true;

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
