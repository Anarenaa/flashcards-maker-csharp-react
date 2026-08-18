using Core.DTOs;
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

        public MySetsController(SetService setService)
        {
            _setService = setService;
        }

        // CRUD ------------------------

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

        [HttpPost]
        public async Task<IActionResult> CreateSet([FromBody] SetCreateDTO setDto)
        {
            var createdSet = await _setService.AddSetAsync(setDto, UserId);
            return CreatedAtAction(nameof(GetMySetById), new { id = createdSet.Id }, createdSet);
        }

        [HttpPut("{setId}")]
        public async Task<IActionResult> UpdateSet(int setId, [FromBody] SetCreateDTO setDto)
        {
            await _setService.UpdateSetAsync(setId, setDto);
            return NoContent();
        }
        [HttpDelete("{setId}")]
        public async Task<IActionResult> DeleteSet(int setId)
        {
            await _setService.DeleteSetAsync(setId);
            return NoContent();
        }
        //-----------------------------

        [HttpPost("{setId}/add-category")]
        public async Task<IActionResult> AddCategoryToSetAsync(int setId, [FromQuery] int categoryId)
        {
            await _setService.AddCategoryToSetAsync(setId, categoryId);
            return NoContent();
        }
        [HttpPost("{setId}/remove-category")]
        public async Task<IActionResult> RemoveCategoryFromSetAsync(int setId, [FromQuery] int categoryId)
        {
            await _setService.RemoveCategoryFromSetAsync(setId, categoryId);
            return NoContent();
        }
    }
}
