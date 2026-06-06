using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace Core.DTOs
{
    public class SetDTO
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Назва обов'язкова")]
        [StringLength(100, ErrorMessage = "Назва занадто довга", MinimumLength = 2)]
        public required string Name { get; set; }
        [StringLength(500, ErrorMessage = "Опис занадто довгий")]
        public string? Description { get; set; }
        public SetType Type { get; set; }
        public string? FromLang { get; set; }
        public string? ToLang { get; set; }
        public bool IsPublic { get; set; }
        public bool IsGenerated { get; set; } = false;
        public string? AvatarUrl { get; set; }
        public string? UserName { get; set; }
        public int FlashcardsCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int Progress { get; set; } = 0; // Одне поле для відсотків (ну сорі по іншому ніяк)
    }
}
