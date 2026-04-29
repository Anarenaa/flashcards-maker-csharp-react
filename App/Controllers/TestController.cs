using Core.DTOs.Practice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services.Interfaces;

namespace App.Controllers
{
    [Authorize]
    [Route("Test")]
    public class TestController(IPracticeService practiceService, IUnitOfWork unitOfWork) : BaseController
    {
        [HttpGet("Map")]
        public async Task<IActionResult> Map(int setId)
        {
            var progress = await practiceService.GetSetProgressAsync(setId, UserId);
            var set = await unitOfWork.Sets.GetByIdAsync(setId);

            ViewBag.SetName = set?.Name ?? "Навчальний сет";
            ViewBag.SetId = setId;

            return View("~/Views/Practic/Map.cshtml", progress);
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(int setId, int mode)
        {
            var activityType = (PracticeActivityType)mode;
            var session = await practiceService.GetPracticeSessionAsync(setId, UserId, activityType);

            if (session == null || session.Flashcards == null || !session.Flashcards.Any())
            {
                TempData["Info"] = $"Помилка: Картки для сету #{setId} не знайдені в режимі {activityType}. Перевірте, чи є картки в самому сеті.";
                return RedirectToAction(nameof(Map), new { setId });
            }

            return View("~/Views/Practic/Index.cshtml", session);
        }

        [HttpPost("SaveResults")]
        public async Task<IActionResult> SaveResults([FromBody] PracticeResultsDTO results)
        {
            await practiceService.SavePracticeResultsAsync(UserId, results);

         
            var firstCardId = results.Results.FirstOrDefault()?.FlashcardId ?? 0;
            var card = await unitOfWork.Flashcards.GetByIdAsync(firstCardId);
            int setId = card?.SetId ?? 0;

            return Ok(new { redirectUrl = Url.Action(nameof(Map), new { setId }) });
        }
    }
}