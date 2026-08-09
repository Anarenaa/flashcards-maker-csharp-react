using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/my-sets")]
    [ApiController]
    public class MySetsController : BaseApiController
    {
        private SetService _setService;
        private FlashcardService _flashcardService;

        public MySetsController(SetService setService, FlashcardService flashcardService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMySets(
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] SetType? setType = null,
            [FromQuery] string? fromLangCode = null,
            [FromQuery] string? searchText = null,
            [FromQuery] string? progress = null)
        {
            var pagedResult = await _setService.GetAllUserSetsPagedAsync(
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
        public async Task<IActionResult> GetMySetById(int id)
        {
            var set = await _setService.GetSetByIdAsync(id, UserId);
            return Ok(set);
        }
        [HttpGet("{setId}/flashcards")]
        public async Task<IActionResult> GetFlashcards(int setId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagedCards = await _flashcardService.GetPagedFlashcardsBySetIdAsync(setId, page, pageSize);
            return Ok(pagedCards);
        }
    }
}
