using Core.DTOs;
using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers.api
{
    [ApiController]
    [Route("api/sets")]
    public class SetsApiController : ControllerBase
    {
        private readonly SetService _setService;

        public SetsApiController(SetService setService)
        {
            _setService = setService;
        }

        // GET: api/sets
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SetDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? searchText)
        {
            // Використовуємо 0 або 1 як currentUserId, оскільки API публічне
            var sets = await _setService.GetAllSetsAsync(0, null, searchText);
            return Ok(sets);
        }
        [HttpGet]
        [Route("progress/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<SetWithProgressDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllWithProgress(int userId, [FromQuery] string? searchText)
        {
            var sets = await _setService.GetSetsWithProgress(userId, null, searchText);
            return Ok(sets);
        }

        [HttpDelete]
        [Route("progress/{userId}")]
        public async Task<IActionResult> DeleteProgress(int userId, int setId)
        {
            try
            {
                await _setService.ResetSetProgressAsync(userId, setId);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: api/sets/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SetDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var set = await _setService.GetSetByIdAsync(id);
                return Ok(set);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/sets
        [HttpPost]
        [ProducesResponseType(typeof(SetDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SetCreateDTO setDto)
        {
            if (setDto == null) return BadRequest();

            // Оскільки авторизації немає, використовуємо заглушку для userId (наприклад, 1)
            var createdSet = await _setService.AddSetAsync(setDto, 1);

            return CreatedAtAction(nameof(GetById), createdSet);
        }

        // PUT: api/sets/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] SetCreateDTO setDto)
        {
            try
            {
                await _setService.UpdateSetAsync(id, setDto);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/sets/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _setService.DeleteSetAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}