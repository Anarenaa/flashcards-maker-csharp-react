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

        [HttpPost("progress-batch")]
        public async Task<IActionResult> GetOverallProgressForFlashcardsBatch([FromBody] List<int> flashcardIds)
        {
            var progress = await _practiceService.GetOverallProgressForFlashcardsBatch(UserId, flashcardIds);
            return Ok(new { progress });
        }
        [HttpPost("reset-batch-progress")]
        public async Task<IActionResult> ResetBatchCardProgressAsync(List<int> flashcardIds)
        {
            await _practiceService.ResetBatchCardProgressAsync(UserId, flashcardIds);
            return Ok();
        }

        [HttpPost("/api/sets/{setId}/sessions")]
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

        [HttpPost("check-answer")]
        public async Task<IActionResult> CheckAnswer(
        [FromQuery] int flashcardId,
        [FromQuery] string userAnswer,
        [FromQuery] PracticeActivityType activityType,
        [FromQuery] bool isReversed)
        {
            var isCorrect = await _practiceService.CheckAnswerAsync(flashcardId, userAnswer, activityType, isReversed);
            return Ok(new { isCorrect });
        }

        [HttpPost("results")]
        public async Task<IActionResult> SaveResults([FromBody] List<PracticeResultDTO> results)
        {
            var incorrectCards = await _practiceService.SavePracticeResultsAsync(UserId, results);
            return Ok(new { incorrectCards });
        }

        [HttpGet("get-limits")]
        public IActionResult GetPracticeLimits()
        {
            var limits = new
            {
                ReviewLimit = PracticeActivityLimit.ReviewLimit,
                QuizLimit = PracticeActivityLimit.QuizLimit,
                MatchingLimit = PracticeActivityLimit.MatchingLimit,
                WritingLimit = PracticeActivityLimit.WritingLimit
            };
            return Ok(limits);
        }
    }
}
