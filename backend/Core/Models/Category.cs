using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Category : BaseModel
    {
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
