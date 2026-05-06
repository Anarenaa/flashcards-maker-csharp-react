using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Repositories.Interfaces;

namespace Services
{
    public class FlashcardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public FlashcardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<FlashcardDTO>> GetFlashcardsBySetIdAsync(int setId)
        {
            var flashcards = await _unitOfWork.Flashcards.GetAllAsync(
                filter: f => f.SetId == setId
            );
            return flashcards.Select(f => new FlashcardDTO
            {
                Id = f.Id,
                Term = f.Term,
                Definition = f.Definition
            });
        }
        public async Task<FlashcardDTO> GetFlashcardByIdAsync(int id)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(id);
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");
            return new FlashcardDTO
            {
                Id = flashcard.Id,
                Term = flashcard.Term,
                Definition = flashcard.Definition
            };
        }
        public async Task CreateFlashcardAsync(int setId, FlashcardDTO flashcardDto)
        {
            var flashcard = new Flashcard
            {
                Term = flashcardDto.Term,
                Definition = flashcardDto.Definition,
                SetId = setId
            };
            await _unitOfWork.Flashcards.AddAsync(flashcard);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task CreateFlashcardsRangeAsync(int setId, IEnumerable<FlashcardDTO> flashcardDtos)
        {
            var flashcards = flashcardDtos.Select(dto => new Flashcard
            {
                Term = dto.Term,
                Definition = dto.Definition,
                SetId = setId
            }).ToList();
            await _unitOfWork.Flashcards.AddRangeAsync(flashcards);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateFlashcardAsync(FlashcardDTO flashcardDto)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(flashcardDto.Id.Value);
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");

            flashcard.Term = flashcardDto.Term;
            flashcard.Definition = flashcardDto.Definition;
            flashcard.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteFlashcardAsync(int id)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(id);
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");
            _unitOfWork.Flashcards.Delete(flashcard);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
