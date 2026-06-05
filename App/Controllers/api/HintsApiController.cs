using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;  

namespace App.Controllers.api
{
    [Route("api/")]
    [ApiController]
    public class HintsApiController : ControllerBase
    {
        private readonly IHintService _hintService;
        public HintsApiController(IHintService hintService)
        {
            _hintService = hintService;
        }
        [HttpGet("get-hint")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHint(
            [FromQuery] string term, 
            [FromQuery] SetType setType, 
            [FromQuery] string? fromLang = "en", 
            [FromQuery] string? toLang = "uk", 
            [FromQuery] string? uiLang = "uk"
        )
        {
            if (string.IsNullOrWhiteSpace(term) || !Enum.IsDefined(typeof(SetType), setType) || string.IsNullOrWhiteSpace(fromLang))
            {
                return BadRequest("Усі параметри є обов'язковими.");
            }
            string hint;
            try
            {
                hint = await _hintService.GetHintAsync(term, setType, fromLang, toLang, uiLang);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Помилка при отриманні підказки: {ex.Message}");
            }

            return Ok(new { text = hint });
        }
    }
}
