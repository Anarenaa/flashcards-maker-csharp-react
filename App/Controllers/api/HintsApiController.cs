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
        public async Task<IActionResult> GetHint([FromQuery] string term, [FromQuery] SetType setType, [FromQuery] string fromLang, [FromQuery] string toLang = "uk")
        {
            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(setType.ToString()) || string.IsNullOrWhiteSpace(fromLang))
            {
                return BadRequest("Усі параметри є обов'язковими.");
            }
            var hint = await _hintService.GetHintAsync(term, setType, fromLang, toLang);
            return Ok(hint);
        }
    }
}
