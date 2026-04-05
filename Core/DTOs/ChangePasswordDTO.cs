using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Введіть вашу пошту")]
        [EmailAddress(ErrorMessage = "Невірний формат пошти")]
        public required string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Введіть новий пароль")]
        [MinLength(6, ErrorMessage = "Пароль повинен містити не менше 6 символів")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Паролі не співпадають")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Підтвердіть новий пароль")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }
    }
}
