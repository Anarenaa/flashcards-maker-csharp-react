using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace App.Controllers.api
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [Route("api/")]
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
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<FlashcardDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateCards([FromForm] CardGenerationRequest request)
        {
            byte[]? imageBytes = null;
            string? mimeType = null;
            List<FlashcardDTO> cards;

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await request.ImageFile.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
                mimeType = request.ImageFile.ContentType;
            }

            try
            {
                cards = await _geminiService.GenerateCardsAsync(request.Prompt, request.Count, imageBytes, mimeType);

                if (cards == null || !cards.Any())
                {
                    return BadRequest("ШІ не зміг згенерувати картки. Спробуйте змінити запит.");
                }

                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Помилка ШІ: " + ex.Message });
            }
        }

        [HttpPost("save-generated-set")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveGeneratedSet([FromBody] SaveSetWithCardsRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (request?.SetDto == null || string.IsNullOrEmpty(request.SetDto.Name))
            {
                return BadRequest("Некоректні дані сету.");
            }

            try
            {
                request.SetDto.IsGenerated = true;
                var createdSet = await _setService.AddSetAsyncWithReturn(request.SetDto, userId);

                if (request.Cards != null && request.Cards.Any())
                {
                    await _flashcardService.CreateFlashcardsRangeAsync(createdSet.Id, request.Cards);
                }

                var resultDto = new SetDetailDTO
                {
                    Id = createdSet.Id,
                    Name = createdSet.Name,
                    Description = createdSet.Description,
                    Type = createdSet.Type,
                    IsPublic = createdSet.IsPublic,
                    IsGenerated = createdSet.IsGenerated,
                    UserName = createdSet.User?.UserName ?? "Користувач",
                    FlashcardsCount = request.Cards?.Count ?? 0,
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Помилка при збереженні в БД: " + ex.Message });
            }
        }
    }
    public class CardGenerationRequest
    {
        public string Prompt { get; set; }
        public int Count { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
    public class SaveSetWithCardsRequest
    {
        public SetDTO SetDto { get; set; }
        public List<FlashcardDTO> Cards { get; set; }
    }
}