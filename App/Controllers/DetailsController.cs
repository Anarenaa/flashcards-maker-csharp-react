using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Route("MySets/[controller]")] 
    public class DetailsController : Controller
    {
        private readonly SetService _setService;
        private readonly FlashcardService _flashcardService;

        public DetailsController(SetService setService, FlashcardService flashcardService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var setDetail = await _setService.GetSetByIdAsync(id);
                return View(setDetail);
            }
            catch
            {
                return RedirectToAction("Index", "MySets");
            }
        }

        [HttpPost("SaveCard")]
        public async Task<IActionResult> SaveCard(int setId, FlashcardDTO cardDto)
        {
            if (cardDto.Id == null || cardDto.Id == 0)
            {
                await _flashcardService.CreateFlashcardAsync(setId, cardDto);
            }
            else
            {
                await _flashcardService.UpdateFlashcardAsync(cardDto);
            }

            // Повертаємось на сторінку цього ж сету
            return RedirectToAction("Index", new { id = setId });
        }

        [HttpPost("DeleteCard")]
        public async Task<IActionResult> DeleteCard(int cardId, int setId)
        {
            await _flashcardService.DeleteFlashcardAsync(cardId);
            return RedirectToAction("Index", new { id = setId });
        }
    }
}