using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Repositories;
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
                filter: f => f.SetId == setId,
                includeProperties: "Set"
            );
            return flashcards.Select(f => new FlashcardDTO
            {
                Id = f.Id,
                Term = f.Term,
                Definition = f.Definition,
                FromLang = f.Set.FromLang!,
                ToLang = f.Set.ToLang!
            });
        }
        public async Task<PagedResult<FlashcardDTO>> GetPagedFlashcardsBySetIdAsync(int setId, int page, int pageSize)
        {
            var pagedResult = await _unitOfWork.Flashcards.GetAllPagedAsync(
                page,
                pageSize,
                filter: f => f.SetId == setId,
                includeProperties: "Set"
            );

            var mappedItems = pagedResult.Items.Select(f => new FlashcardDTO
            {
                Id = f.Id,
                Term = f.Term,
                Definition = f.Definition,
                FromLang = f.Set.FromLang!,
                ToLang = f.Set.ToLang!
            }).ToList();

            return new PagedResult<FlashcardDTO>(
                mappedItems,
                pagedResult.TotalItems,
                page,
                pageSize
            );
        }
        public async Task<FlashcardDTO> GetFlashcardByIdAsync(int id)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(id, includeProperties: "Set");
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");
            return new FlashcardDTO
            {
                Id = flashcard.Id,
                Term = flashcard.Term,
                Definition = flashcard.Definition,
                FromLang = flashcard.Set.FromLang!,
                ToLang = flashcard.Set.ToLang!
            };
        }
        private async Task updateTimeInSet(int setId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set != null)
            {
                set.UpdatedAt = DateTime.UtcNow;
            }
        }
        public async Task<FlashcardDTO> CreateFlashcardAsync(int setId, FlashcardDTO flashcardDto)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set == null)
                throw new NotFoundException("Сет не знайдено.");

            var flashcard = new Flashcard
            {
                Term = flashcardDto.Term,
                Definition = flashcardDto.Definition,
                SetId = setId
            };
            await _unitOfWork.Flashcards.AddAsync(flashcard);
            await updateTimeInSet(setId);

            await _unitOfWork.SaveChangesAsync();

            return new FlashcardDTO
            {
                Id = flashcard.Id,
                Term = flashcard.Term,
                Definition = flashcard.Definition,
                FromLang =  set?.FromLang,
                ToLang = set?.ToLang
            };
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
            await updateTimeInSet(setId);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateFlashcardAsync(int flashcardId, FlashcardDTO flashcardDto)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(flashcardId);
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");

            flashcard.Term = flashcardDto.Term;
            flashcard.Definition = flashcardDto.Definition;
            flashcard.UpdatedAt = DateTime.UtcNow;

            await updateTimeInSet(flashcard.SetId);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteFlashcardAsync(int id)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(id);
            if (flashcard == null)
                throw new NotFoundException("Картка не знайдена.");
            _unitOfWork.Flashcards.Delete(flashcard);

            await updateTimeInSet(flashcard.SetId);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
