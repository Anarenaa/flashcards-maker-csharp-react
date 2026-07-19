using Microsoft.AspNetCore.Mvc;
using Core.DTOs;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/sets/{setId}/flashcards/{flashcardId}/contexts")]
    [ApiController]
    public class FlashcardContextsController : BaseApiController
    {
        private readonly FlashcardContextService _flashcardContextService;

        public FlashcardContextsController(FlashcardContextService flashcardContextService)
        {
            _flashcardContextService = flashcardContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int flashcardId)
        {
            var contexts = await _flashcardContextService.GetAllFlashcardContextsAsync(flashcardId);
            return Ok(contexts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var context = await _flashcardContextService.GetFlashcardContextByIdAsync(id);
            if (context == null) return NotFound();
            return Ok(context);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int flashcardId, [FromBody] FlashcardContextDTO dto)
        {
            await _flashcardContextService.CreateFlashcardContextAsync(flashcardId, dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FlashcardContextDTO dto)
        {
            await _flashcardContextService.UpdateFlashcardContextAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _flashcardContextService.DeleteFlashcardContextAsync(id);
            return NoContent();
        }
    }
}