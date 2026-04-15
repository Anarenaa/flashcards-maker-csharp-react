using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class UpdateAvatarUrlViewModel
    {
        [Url(ErrorMessage = "Введіть коректний URL зображення")]
        [StringLength(500, ErrorMessage = "URL занадто довгий")]
        [Display(Name = "URL аватара")]
        public string AvatarUrl { get; set; }
    }
}
