using System.ComponentModel.DataAnnotations;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace Core.DTOs
{
    public class SetAIPromtCreateDTO
    {
        public SetType Type { get; set; }
        public required string FromLang { get; set; }
        public required string ToLang { get; set; }

        [Required(ErrorMessage = "Промт обов'язковий")]
        [StringLength(1000, ErrorMessage = "Промт занадто довгий")]
        public required string Prompt { get; set; }

        public IFormFile? File { get; set; }
        public int? CardsCount { get; set; }

    }
}
