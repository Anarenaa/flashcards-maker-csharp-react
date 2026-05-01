using Core.DTOs;

namespace Services.Interfaces
{
    public interface IGeminiService
    {
        Task<List<FlashcardDTO>> GenerateCardsAsync(string prompt, byte[]? imageBytes = null, string? mimeType = null);
    }
}
