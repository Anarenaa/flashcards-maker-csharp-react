using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        [RegularExpression(@"^(?=.*\d).+$",
            ErrorMessage = "Пароль має містити хоча б одну цифру")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public required string ConfirmPassword { get; set; }
    }
}