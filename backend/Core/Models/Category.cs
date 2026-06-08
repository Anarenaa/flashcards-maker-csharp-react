using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Category : IHasCreationDate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
