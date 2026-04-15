using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace App.Models
{
    public class UpdateAvatarFileViewModel
    {
        [DataType(DataType.Upload)]
        [Display(Name = "Аватар")]
        [Required(ErrorMessage = "Оберіть файл для завантаження")]
        [FileExtensions(Extensions = "jpg,jpeg,png,webp", ErrorMessage = "Дозволені формати: JPG, JPEG, PNG, WebP")]
        public IFormFile AvatarFile { get; set; }
    }
}
