using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace Core.DTOs
{
    public class SetCreateDTO
    {
        [Required(ErrorMessage = "Назва обов'язкова")]
        [StringLength(100, ErrorMessage = "Назва занадто довга", MinimumLength = 2)]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Опис занадто довгий")]
        public string? Description { get; set; }

        public SetType Type { get; set; } 
        public required string FromLang { get; set; }
        public required string ToLang { get; set; }
        public bool IsPublic { get; set; }
    }
}
