using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool IsPublic { get; set; }
        public string? UserName { get; set; }
        public int FlashcardsCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
