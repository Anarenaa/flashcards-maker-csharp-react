using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class CardProgress
    {
        public int FlashcardId { get; set; }
        public Flashcard Flashcard { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        [Range(0.0, 1.0)]
        public float Progress { get; set; } = 0.0f;
        public DateTime LastReview { get; set; }
        public DateTime NextReview { get; set; }
    }
}
