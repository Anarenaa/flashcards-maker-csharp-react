using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Core.Models
{
    public class User : IdentityUser<int>
    {
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }
}
