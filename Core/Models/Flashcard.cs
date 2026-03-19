using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Flashcard : BaseModel
    {
        [Required]
        [StringLength(1000)]
        public required string Term { get; set; }
        [Required]
        [StringLength(1000)]
        public required string Definition { get; set; }

        [Required]
        public int SetId { get; set; }
        public required Set Set { get; set; }
    }
}
