using Core.DTOs;
using Core.Models;

namespace Services.Interfaces
{
    public interface IGeminiService
    {
        Task<List<FlashcardDTO>> GenerateCardsAsync(SetDTO setDto, int count, byte[]? imageBytes = null, string? mimeType = null);
        Task<string?> GenerateSimpleHintAsync(string term, string targetLang, SetType type);
    }
}
