using Core.DTOs;
using Core.Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class FlashcardContextService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiService _geminiService;
        public FlashcardContextService(IUnitOfWork unitOfWork, IGeminiService geminiService)
        {
            _unitOfWork = unitOfWork;
            _geminiService = geminiService;
        }
        public async Task<List<FlashcardContextDTO>> GetAllFlashcardContextsAsync(int flashcardId)
        {
            var contexts = await _unitOfWork.FlashcardContexts
                .GetAllAsync(filter: fc => fc.FlashcardId == flashcardId);

            return contexts.Select(fc => new FlashcardContextDTO
            {
                Id = fc.Id,
                Sentence = fc.Sentence,
                Translation = fc.Translation,
                IsGenerated = fc.IsGenerated
            }).ToList();
        }
        public async Task<FlashcardContextDTO?> GetFlashcardContextByIdAsync(int id)
        {
            var context = await _unitOfWork.FlashcardContexts.GetByIdAsync(id);
            if (context == null)
                return null;

            return new FlashcardContextDTO
            {
                Id = context.Id,
                Sentence = context.Sentence,
                Translation = context.Translation,
                IsGenerated = context.IsGenerated
            };
        }
        public async Task CreateFlashcardContextAsync(int flashcardId, FlashcardContextDTO dto)
        {
            var flashcardContext = new FlashcardContext
            {
                Sentence = dto.Sentence,
                Translation = dto.Translation,
                FlashcardId = flashcardId
            };
            await _unitOfWork.FlashcardContexts.AddAsync(flashcardContext);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateFlashcardContextAsync(int flashcardContextId, FlashcardContextDTO dto)
        {
            var flashcardContextToUpdate = await _unitOfWork.FlashcardContexts.GetByIdAsync(flashcardContextId);
            if (flashcardContextToUpdate == null)
            {
                throw new Exception($"FlashcardContext with ID {flashcardContextId} not found.");
            }

            flashcardContextToUpdate.Sentence = dto.Sentence;
            flashcardContextToUpdate.Translation = dto.Translation;
            flashcardContextToUpdate.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteFlashcardContextAsync(int flashcardContextId)
        {
            var flashcardContext = await _unitOfWork.FlashcardContexts.GetByIdAsync(flashcardContextId);
            if (flashcardContext == null)
            {
                throw new Exception($"FlashcardContext with ID {flashcardContextId} not found.");
            }
            _unitOfWork.FlashcardContexts.Delete(flashcardContext);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task<List<FlashcardContextDTO>> GenerateAndSaveSingleContextAsync(int flashcardId)
        {
            var generatedContexts = await _geminiService.GenerateContextsForFlashcardAsync(flashcardId);

            if (generatedContexts == null || !generatedContexts.Any())
                return new List<FlashcardContextDTO>();

            var contextsToSave = generatedContexts.Select(dto => new FlashcardContext
            {
                Sentence = dto.Sentence,
                Translation = dto.Translation,
                FlashcardId = flashcardId,
                IsGenerated = true
            }).ToList();

            await _unitOfWork.FlashcardContexts.AddRangeAsync(contextsToSave);
            await _unitOfWork.SaveChangesAsync();

            for (int i = 0; i < generatedContexts.Count; i++)
            {
                generatedContexts[i].Id = contextsToSave[i].Id;
                generatedContexts[i].IsGenerated = true;
            }

            return generatedContexts;
        }
    }
}
