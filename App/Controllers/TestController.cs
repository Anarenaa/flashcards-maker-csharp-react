using Core.DTOs.Practice;
using Core.Models;
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
        [HttpGet("Map/{setId}")]
        public async Task<IActionResult> Map(int setId)
        {
            var progress = await practiceService.GetSetProgressAsync(setId, UserId);
            var set = await unitOfWork.Sets.GetByIdAsync(setId);

            if (set == null) return NotFound();

            ViewBag.SetName = set.Name;
            ViewBag.SetId = setId;
            ViewBag.CurrentProgress = progress.OverallProgress;

            return View("~/Views/Practic/Map.cshtml", progress);
        }

        [HttpGet("Index/{setId}/{mode}")]
        public async Task<IActionResult> Index(int setId, int mode)
        {
            var activityType = (PracticeActivityType)mode;

            var session = await practiceService.GetPracticeSessionAsync(setId, UserId, activityType);

            var progress = await practiceService.GetSetProgressAsync(setId, UserId);

            if (session == null || session.Flashcards == null || !session.Flashcards.Any())
            {
                TempData["Info"] = "Для цього режиму поки немає доступних карток.";
                return RedirectToAction(nameof(Map), new { setId });
            }

            ViewBag.CurrentProgress = progress.OverallProgress;
            ViewBag.Mode = mode;

            return View("~/Views/Practic/Index.cshtml", session);
        }

        [HttpPost("SaveResults")]
        public async Task<IActionResult> SaveResults([FromBody] PracticeResultsDTO results)
        {
            await practiceService.SavePracticeResultsAsync(UserId, results);

            // Знаходимо setId, щоб повернутися на карту
            var firstCardId = results.Results.FirstOrDefault()?.FlashcardId ?? 0;
            var card = await unitOfWork.Flashcards.GetByIdAsync(firstCardId);
            int setId = card?.SetId ?? 0;

            // ПЕРЕНАПРАВЛЕННЯ НА КАРТУ
            return Ok(new { redirectUrl = $"/Test/Map/{setId}" });
        }
    }
}