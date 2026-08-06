using System.Threading.Tasks;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Practice;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetsController : BaseApiController
    {
        private readonly SetService _setService;
        private readonly FlashcardService _flashcardService;
        private readonly IPracticeService _practiceService;
        public SetsController(SetService setService, FlashcardService flashcardService, IPracticeService practiceService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
            _practiceService = practiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSets(
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] SetType? setType = null,
            [FromQuery] string? fromLangCode = null,
            [FromQuery] string? searchText = null,
            [FromQuery] string? progress = null)
        {
            var pagedResult = await _setService.GetAllSetsAsync(
                page: page,
                perPage: perPage,
                currentUserId: UserId,
                categoryId: categoryId,
                setType: setType,
                fromLangCode: fromLangCode,
                searchText: searchText,
                progress: progress
            );

            return Ok(pagedResult);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSetById(int id)
        {
            var set = await _setService.GetSetByIdAsync(id);
            if (set == null)
            {
                return NotFound();
            }
            return Ok(set);
        }
        [HttpGet("types")]
        public IActionResult GetSetTypes()
        {
            var setTypes = _setService.GetSetTypes();
            return Ok(setTypes);
        }
        [HttpGet("{setId}/flashcards")]
        public async Task<IActionResult> GetFlashcards(int setId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagedCards = await _flashcardService.GetPagedFlashcardsBySetIdAsync(setId, page, pageSize);
            return Ok(pagedCards);
        }

        [HttpGet("{setId}/get-progress")]
        public async Task<IActionResult> GetSetProgress(int setId)
        {
            return await _practiceService.GetSingleSetProgressAsync(UserId, setId) is float progress
                ? Ok(new { progress })
                : NotFound();
        }
        [HttpPost("{setId}/reset-set-progress")]
        public async Task<IActionResult> ResetSetProgress(int setId)
        {
            await _practiceService.ResetSetProgressAsync(UserId, setId);
            return Ok();
        }
    }
}
