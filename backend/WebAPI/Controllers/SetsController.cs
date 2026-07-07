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
        public SetsController(SetService setService)
        {
            _setService = setService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSets(
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] SetType? setType = null,
            [FromQuery] string? fromLangCode = null,
            [FromQuery] string? searchText = null)
        {
            var pagedResult = await _setService.GetAllSetsAsync(
                page: page,
                perPage: perPage,
                currentUserId: UserId,
                categoryId: categoryId,
                setType: setType,
                fromLangCode: fromLangCode,
                searchText: searchText
            );

            return Ok(pagedResult);
        }
    }
}
