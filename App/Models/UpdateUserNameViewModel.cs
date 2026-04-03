using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class UpdateUserNameViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно бути від 3 до 50 символів")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9_\-.]+$", ErrorMessage = "Ім'я може містити лише літери, цифри та символи _ - .")]
        [Display(Name = "Ім'я користувача")]
        public string UserName { get; set; }
    }
}
