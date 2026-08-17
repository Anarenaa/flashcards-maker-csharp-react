using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [Route("api/sets/{setId}/[controller]")]
    [ApiController]
    public class FlashcardsController : BaseApiController
    {
        private readonly FlashcardContextService _flashcardContextService;
        private readonly FlashcardService _flashcardService;
        private readonly SetService _setService;
        private readonly IHintService _hintService;
        public FlashcardsController(
            FlashcardContextService flashcardContextService, 
            FlashcardService flashcardService, 
            SetService setService,
            IHintService hintService)
        {
            _flashcardContextService = flashcardContextService;
            _flashcardService = flashcardService;
            _setService = setService;
            _hintService = hintService;
        }

        // CRUD ------------------------
        [HttpGet]
        public async Task<IActionResult> GetFlashcards(int setId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagedCards = await _flashcardService.GetPagedFlashcardsBySetIdAsync(setId, page, pageSize);
            return Ok(pagedCards);
        }

        [HttpGet("{flashcardId}")]
        public async Task<IActionResult> GetFlashcardById(int flashcardId)
        {
            var flashcard = await _flashcardService.GetFlashcardByIdAsync(flashcardId);
            if (flashcard == null)
            {
                return NotFound();
            }
            return Ok(flashcard);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFlashcard(int setId, [FromBody] FlashcardDTO flashcardDto)
        {
            var createdFlashcard = await _flashcardService.CreateFlashcardAsync(setId, flashcardDto);
            
            return CreatedAtAction(
                nameof(GetFlashcardById),
                new { setId = setId, flashcardId = createdFlashcard.Id },
                createdFlashcard
            );
        }

        [HttpPut("{flashcardId}")]
        public async Task<IActionResult> UpdateFlashcard(int flashcardId, [FromBody] FlashcardDTO flashcardDto)
        {
            await _flashcardService.UpdateFlashcardAsync(flashcardId, flashcardDto);
            return NoContent();
        }

        [HttpDelete("{flashcardId}")]
        public async Task<IActionResult> DeleteFlashcard(int flashcardId)
        {
            await _flashcardService.DeleteFlashcardAsync(flashcardId);
            return NoContent();
        }
        // ------------------------------

        [HttpPost("{flashcardId}/generate-context")]
        public async Task<IActionResult> Generate(int flashcardId)
        {
            var newContexts = await _flashcardContextService.GenerateAndSaveSingleContextAsync(flashcardId);
            return Ok(newContexts);
        }

        [HttpPost("hint")]
        public async Task<IActionResult> GetHint(int setId, [FromQuery] string term)
        {
            var set = await _setService.GetSetByIdAsync(setId);
            if (set == null)
            {
                return NotFound("Сет не знайдено.");
            }

            var hint = await _hintService.GetHintAsync(set.Type, term, set.FromLang!, set.ToLang!);
            if (hint == null)
            {
                return NotFound();
            }
            return Ok(hint);
        }
    }
}
