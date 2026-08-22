using Core.DTOs;
using Core.DTOs.Practice;
using Core.Models;

namespace Services.Interfaces
{
    public interface IGeminiService
    {
        Task<(SetCreateDTO? SetDto, List<FlashcardDTO>? Flashcards)> GenerateSetWithFlashcardsAsync(SetAIPromtCreateDTO requestDto);
        Task<string?> GenerateSimpleHintAsync(string term, string targetLang, SetType type);
        Task<List<FlashcardContextDTO>?> GenerateContextsForFlashcardAsync(int cardId);
    }
}
