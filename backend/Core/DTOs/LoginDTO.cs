using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Поле обов'язкове")]
        public required string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        public required string Password { get; set; }
    }
}