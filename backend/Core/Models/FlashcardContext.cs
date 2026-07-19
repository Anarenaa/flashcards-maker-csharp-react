using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class FlashcardContext : IHasCreationDate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FlashcardId { get; set; }
        public Flashcard Flashcard { get; set; }
        public required string Sentence { get; set; }
        public required string Translation { get; set; }
        public bool IsGenerated { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }
}
