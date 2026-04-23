using Core.DTOs.Practice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services.Interfaces;

namespace App.Controllers
{
    [Authorize]
    public class TestController(IPracticeService practiceService, IUnitOfWork unitOfWork) : BaseController
    {
        // Карта шляху практики
        public async Task<IActionResult> Map(int setId)
        {
            var progress = await practiceService.GetSetProgressAsync(setId, UserId);
            var set = await unitOfWork.Sets.GetByIdAsync(setId);

            ViewBag.SetName = set?.Name ?? "Навчальний сет";
            ViewBag.SetId = setId;

            return View("~/Views/Practic/Map.cshtml", progress);
        }
        // Запуск конкретного режиму з карти
        public async Task<IActionResult> Index(int setId, int mode)
        {
            var activityType = (PracticeActivityType)mode;
            var session = await practiceService.GetPracticeSessionAsync(setId, UserId, activityType);

            if (session == null || !session.Flashcards.Any())
            {
                TempData["Info"] = "Цей етап уже пройдено або картки відсутні!";
                return RedirectToAction(nameof(Map), new { setId });
            }

            return View("~/Views/Practic/Index.cshtml", session);
        }

        [HttpPost]
        public async Task<IActionResult> SaveResults([FromBody] PracticeResultsDTO results)
        {
            await practiceService.SavePracticeResultsAsync(UserId, results);
            var setId = results.Results.FirstOrDefault()?.FlashcardId;
            return Ok(new { redirectUrl = Url.Action(nameof(Map), new { setId = results.Results.Any() ? results.Results.First().FlashcardId : 0 }) });
        }
    }
}