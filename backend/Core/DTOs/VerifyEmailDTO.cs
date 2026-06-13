using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Введіть вашу пошту")]
        [EmailAddress(ErrorMessage = "Невірний формат пошти")]
        public required string Email { get; set; }
    }
}
