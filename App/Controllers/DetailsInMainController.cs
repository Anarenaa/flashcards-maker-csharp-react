using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Authorize]
    public class DetailsInMainController : BaseController
    {
        private readonly SetService _setService;
        private readonly FlashcardService _flashcardService;
        private readonly CategoryService _categoryService;

        public DetailsInMainController(SetService setService, FlashcardService flashcardService, CategoryService categoryService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
            _categoryService = categoryService;
        }

        [HttpGet("Details/Sets/{id}")]
        public async Task<IActionResult> Sets(int id)
        {
            try
            {
                var setDetail = await _setService.GetSetByIdAsync(id);
                if (setDetail == null) return NotFound();

                // ПЕРЕВІРКА: чи я власник? (UserId з BaseController)
               // ViewBag.IsOwner = setDetail.OwnerId == UserId;

                ViewBag.AllCategories = await _categoryService.GetAllCategoriesAsync();

                return View(setDetail);
            }
            catch
            {
                return RedirectToAction("Index", "Main");
            }
        }
    }
}