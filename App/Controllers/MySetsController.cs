using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    public class MySetsController : Controller
    {
        private readonly SetService _setService;

        public MySetsController(SetService setService)
        {
            _setService = setService;
        }

        // Відображення списку
        public async Task<IActionResult> Index()
        {
            var sets = await _setService.GetAllSetsAsync([]);
            return View(sets);
        }

        // Створення нового сету
        [HttpPost]
        public async Task<IActionResult> Create(SetDTO setDto)
        {
            if (!ModelState.IsValid)
            {
                var allSets = await _setService.GetAllSetsAsync([]);
                return View("Index", allSets); // Повертаємо помилки, якщо назва порожня
            }

            await _setService.AddSetAsync(setDto); // ВИКЛИК СЕРВІСУ
            return RedirectToAction(nameof(Index)); // Перезавантаження сторінки
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SetDTO setDto)
        {
            if (ModelState.IsValid)
            {
                await _setService.UpdateSetAsync(setDto); // ВИКЛИК СЕРВІСУ
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _setService.DeleteSetAsync(id); // ВИКЛИК СЕРВІСУ
            return RedirectToAction(nameof(Index));
        }
    }
}