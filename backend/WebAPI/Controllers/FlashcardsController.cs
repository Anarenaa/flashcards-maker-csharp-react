using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/sets/{setId}/[controller]")]
    [ApiController]
    public class FlashcardsController : BaseApiController
    {
        private readonly FlashcardContextService _flashcardContextService;
        private readonly FlashcardService _flashcardService;
        public FlashcardsController(FlashcardContextService flashcardContextService, FlashcardService flashcardService)
        {
            _flashcardContextService = flashcardContextService;
            _flashcardService = flashcardService;
        }

        [HttpPost("{flashcardId}/generate-context")]
        public async Task<IActionResult> Generate(int flashcardId)
        {
            var newContexts = await _flashcardContextService.GenerateAndSaveSingleContextAsync(flashcardId);
            return Ok(newContexts);
        }

    }
}
