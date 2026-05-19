using Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Exceptions;

namespace App.Controllers
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
        public async Task<IActionResult> Create([FromBody] SetDTO setDto)
        {
            if (setDto == null) return BadRequest();

            // Оскільки авторизації немає, використовуємо заглушку для userId (наприклад, 1)
            var createdSet = await _setService.AddSetAsyncWithReturn(setDto, 1);

            return CreatedAtAction(nameof(GetById), new { id = createdSet.Id }, createdSet);
        }

        // PUT: api/sets/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] SetDTO setDto)
        {
            if (setDto == null || id != setDto.Id) return BadRequest();

            try
            {
                await _setService.UpdateSetAsync(setDto);
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