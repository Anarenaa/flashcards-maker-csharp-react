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
        public async Task<IActionResult> Index(int setId, int mode, int currentIndex = 0)
        {
            var activityType = (PracticeActivityType)mode;

            var session = await practiceService.GetPracticeSessionAsync(setId, UserId, activityType, currentIndex);

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
            if (results?.Results == null || !results.Results.Any())
            {
                return Ok(new { redirectUrl = "/Main" });
            }

            await practiceService.SavePracticeResultsAsync(UserId, results);

            await unitOfWork.SaveChangesAsync();

            var firstCardId = results.Results.First().FlashcardId;
            var card = await unitOfWork.Flashcards.GetByIdAsync(firstCardId);
            int setId = card?.SetId ?? 0;

            return Ok(new { redirectUrl = $"/Test/Map/{setId}" });
        }
    }
}