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
        private CategoryService _categoryService;

        public MySetsController(SetService setService, CategoryService categoryService)
        {
            _setService = setService;
            _categoryService = categoryService;
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
            var pagedResult = await _setService.GetAllUserSetsAsync(
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
    }
}
