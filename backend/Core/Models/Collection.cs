using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Collection : BaseModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }
        public string? Description { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
