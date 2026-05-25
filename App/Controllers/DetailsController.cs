using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Route("MySets/[controller]")]
    [Authorize]
    public class DetailsController : BaseController
    {
        private readonly SetService _setService;
        private readonly FlashcardService _flashcardService;
        private readonly CategoryService _categoryService;

        public DetailsController(
            SetService setService,
            FlashcardService flashcardService,
            CategoryService categoryService)
        {
            _setService = setService;
            _flashcardService = flashcardService;
            _categoryService = categoryService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            if (await _setService.IsSetMine(id, UserId) == false)
                return Forbid();

            var setDetail = await _setService.GetSetByIdAsync(id);
            ViewBag.AllCategories = await _categoryService.GetAllCategoriesAsync();
            return View(setDetail);
        }
        
        // --- РОБОТА З КАРТКАМИ ---
        [HttpPost("SaveCard")]
        public async Task<IActionResult> SaveCard(int setId, FlashcardDTO cardDto)
        {
            if (cardDto.Id == null || cardDto.Id == 0)
                await _flashcardService.CreateFlashcardAsync(setId, cardDto);
            else
                await _flashcardService.UpdateFlashcardAsync(cardDto);

            var setDetail = await _setService.GetSetByIdAsync(setId);
            if (setDetail != null)
            {
                var setUpdate = new SetCreateDTO
                {
                    Name = setDetail.Name,
                    Description = setDetail.Description,
                    IsPublic = setDetail.IsPublic
                };
                await _setService.UpdateSetAsync(setId, setUpdate);
            }

            return RedirectToAction("Index", new { id = setId });
        }
        [HttpPost("DeleteCard")]
        public async Task<IActionResult> DeleteCard(int cardId, int setId)
        {
            // 1. Видаляємо саму картку через сервіс
            await _flashcardService.DeleteFlashcardAsync(cardId);

            // 2. СИНХРОНІЗАЦІЯ ЧАСУ: Оновлюємо дату останньої зміни сету
            var setDetail = await _setService.GetSetByIdAsync(setId);
            if (setDetail != null)
            {
                var setUpdate = new SetCreateDTO
                {
                    Name = setDetail.Name,
                    Description = setDetail.Description,
                    IsPublic = setDetail.IsPublic,
                };
                await _setService.UpdateSetAsync(setId, setUpdate);
            }

            // 3. Повертаємось назад у цей же сет
            return RedirectToAction("Index", new { id = setId });
        }
        // --- РОБОТА З КАТЕГОРІЯМИ ---

        [HttpPost("SaveCategory")]
        public async Task<IActionResult> SaveCategory(int setId, int? selectedCategoryId, string? newCategoryName)
        {
            int categoryId = 0;
            if (!string.IsNullOrWhiteSpace(newCategoryName))
            {
                var userCats = await _categoryService.GetAllCategoriesAsync();
                var existing = userCats.FirstOrDefault(c => c.Name.Equals(newCategoryName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    var newCat = new CategoryDTO { Name = newCategoryName.Trim() };
                    await _categoryService.CreateCategoryAsync(newCat);
                    
                    var updated = await _categoryService.GetAllCategoriesAsync();
                    categoryId = updated.First(c => c.Name.Equals(newCategoryName.Trim())).Id.Value;
                }
                else { categoryId = existing.Id.Value; }
            }
         
            else if (selectedCategoryId.HasValue && selectedCategoryId != 0)
            {
                categoryId = selectedCategoryId.Value;
            }

            if (categoryId != 0)
            {
                try { await _setService.AddCategoryToSetAsync(setId, categoryId); } catch {  }
            }

            return RedirectToAction("Index", new { id = setId });
        }

        [HttpPost("EditCategoryName")]
        public async Task<IActionResult> EditCategoryName(int setId, int categoryId, string updatedName)
        {
            if (!string.IsNullOrWhiteSpace(updatedName))
            {
                await _categoryService.UpdateCategoryAsync(new CategoryDTO { Id = categoryId, Name = updatedName });
            }
            return RedirectToAction("Index", new { id = setId });
        }
        [HttpPost("RemoveCategory")]
        public async Task<IActionResult> RemoveCategory(int setId, int categoryId)
        {
           
            await _setService.RemoveCategoryFromSetAsync(setId, categoryId);

            return RedirectToAction("Index", new { id = setId });
        }
        [HttpPost("DeleteCategoryGlobal")]
        public async Task<IActionResult> DeleteCategoryGlobal(int setId, int categoryId)
        {
  
            await _categoryService.DeleteCategoryAsync(categoryId);

            return RedirectToAction("Index", new { id = setId });
        }

    }
}