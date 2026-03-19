using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class User : BaseModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public required string Email { get; set; }
        public string? PasswordHash { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
