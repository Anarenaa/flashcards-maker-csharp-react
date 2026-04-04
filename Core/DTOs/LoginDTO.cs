using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class LoginDto
    {
        [Required()]
        public required string UserNameOrEmail { get; set; }

        [Required]
        public required string Password { get; set; }
        [Required]
        public bool RememberMe { get; set; }
    }
}