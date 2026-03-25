using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers
{
    public class MySetsController : Controller
    {
        private readonly SetService _setService;
        private readonly CollectionService _collectionService; // Додаємо сервіс колекцій

        // Один конструктор для всіх сервісів
        public MySetsController(SetService setService, CollectionService collectionService)
        {
            _setService = setService;
            _collectionService = collectionService;
        }

        // ТІЛЬКИ ОДИН МЕТОД INDEX
        // Обробляє пошук, сортування та завантаження списку колекцій
        public async Task<IActionResult> Index(string? searchText, string sortOrder = "newest")
        {
            // 1. Отримуємо сети користувача (враховуючи пошук)
            var sets = await _setService.GetAllUserSetsAsync(null, null, searchText);

            // 2. Логіка сортування
            sets = sortOrder switch
            {
                "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
                "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
                _ => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            };

            // 3. Дані для фільтрів та модалки в інтерфейсі
            ViewData["CurrentFilter"] = searchText;
            ViewData["CurrentSort"] = sortOrder;

            // Отримуємо папки для модалки "Додати в колекцію" (userId = 1)
            ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(1);

            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SetDTO setDto)
        {
            if (!ModelState.IsValid)
            {
                // Якщо помилка валідації, повертаємо Index з усіма даними
                return await Index(null, "newest");
            }

            await _setService.AddSetAsync(setDto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SetDTO setDto)
        {
            if (ModelState.IsValid)
            {
                await _setService.UpdateSetAsync(setDto);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _setService.DeleteSetAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Метод для додавання в колекцію (викликається з модалки)
        [HttpPost]
        public async Task<IActionResult> AddToCollection(int setId, int collectionId)
        {
            if (setId != 0 && collectionId != 0)
            {
                await _setService.AddSetToCollectionAsync(setId, collectionId);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}