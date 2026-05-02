using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace App.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiApiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly FlashcardService _flashcardService;
        private readonly SetService _setService;

        public GeminiApiController(
            IGeminiService geminiService,
            FlashcardService flashcardService,
            SetService setService)
        {
            _geminiService = geminiService;
            _flashcardService = flashcardService;
            _setService = setService;
        }

        [HttpPost("generate-cards")]
        [ProducesResponseType(typeof(FlashcardDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> GenerateCards([FromQuery] string topic, IFormFile? image)
        {
            byte[]? imageBytes = null;
            string? mimeType = null;
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
                mimeType = image.ContentType;
            }
            var cards = await _geminiService.GenerateCardsAsync(topic, imageBytes, mimeType);
            if (cards == null || !cards.Any())
            {
                return BadRequest("ШІ не зміг згенерувати картки. Спробуйте змінити запит.");
            }
            return Ok(cards);
        }

        [HttpPost("generate-new-set")]
        [ProducesResponseType(typeof(SetDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> GenerateWithNewSet([FromQuery] int userId, [FromQuery] string setName, [FromQuery] string topic, IFormFile? image)
        {
            var newSetDto = new SetDTO { Name = setName, Description = topic };

            var createdSet = await _setService.AddSetAsyncWithReturn(newSetDto, userId);

            byte[]? imageBytes = null;
            string? mimeType = null;

            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
                mimeType = image.ContentType;
            }

            var cards = await _geminiService.GenerateCardsAsync(topic, imageBytes, mimeType);

            if (cards != null && cards.Any())
            {
                await _flashcardService.CreateFlashcardsRangeAsync(createdSet.Id, cards);
            }

            var resultDto = new SetDetailDTO
            {
                Id = createdSet.Id,
                Name = createdSet.Name,
                Description = createdSet.Description,
                IsPublic = createdSet.IsPublic,
                UserName = createdSet.User.UserName,
                FlashcardsCount = cards.Count,
                Flashcards = createdSet.Flashcards.Select(f => new FlashcardDTO
                {
                    Id = f.Id,
                    Term = f.Term,
                    Definition = f.Definition
                }).ToList(),
                CreatedAt = createdSet.CreatedAt,
                LastUpdatedAt = createdSet.UpdatedAt
            };
            return Ok(resultDto);
        }
    }
}