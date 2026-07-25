using Core.DTOs;
using Core.DTOs.Practice;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeController : BaseApiController
    {
        private readonly IPracticeService _practiceService;

        public PracticeController(IPracticeService practiceService)
        {
            _practiceService = practiceService;
        }

        [HttpPost("sets/{setId}/sessions")]
        public async Task<IActionResult> StartSession(
            int setId,
            [FromQuery] PracticeActivityType? mode,
            [FromQuery] bool isReversed,
            [FromBody] List<FlashcardDTO> flashcards)
        {
            var session = await _practiceService.GetPracticeSessionAsync(flashcards, setId, UserId, mode, isReversed);
            if (session == null || !session.Flashcards.Any())
                return Ok(new { completed = true });

            return Ok(session);
        }
    }
}
