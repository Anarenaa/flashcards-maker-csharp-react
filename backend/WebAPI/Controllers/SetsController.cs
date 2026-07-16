using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetsController : BaseApiController
    {
        private readonly SetService _setService;
        private readonly FlashcardService _flashcardService;
        public SetsController(SetService setService, FlashcardService flashcardService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
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
    }
}
