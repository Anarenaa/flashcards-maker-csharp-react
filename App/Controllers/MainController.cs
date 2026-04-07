using System;
using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Authorize]
    public class MainController : BaseController
    {
        private readonly SetService _setService;
        private readonly CollectionService _collectionService;

        public MainController(SetService setService, CollectionService collectionService)
        {
            _setService = setService;
            _collectionService = collectionService;
        }

        public async Task<IActionResult> Index(string? searchText, string sortOrder = "newest")
        {
            // 1. Отримуємо всі публічні сети з урахуванням пошуку
            var sets = await _setService.GetAllSetsAsync(null, searchText);

            // 2. Логіка сортування
            sets = sortOrder switch
            {
                "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
                "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
                _ => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            };

            // 3. Передаємо дані у View
            ViewData["CurrentFilter"] = searchText;
            ViewData["CurrentSort"] = sortOrder;

            // Завантажуємо колекції для модалки збереження
            if (!string.IsNullOrEmpty(UserId))
            {
                ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(int.Parse(UserId));
            }

            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCollection(int setId, int? collectionId, string? newCollectionName)
        {
            int finalCollectionId = collectionId ?? 0;

            // Створення нової колекції, якщо вписана назва
            if (!string.IsNullOrWhiteSpace(newCollectionName))
            {
                var newCol = new CollectionDTO { Name = newCollectionName.Trim() };
                await _collectionService.CreateCollectionAsync(newCol, int.Parse(UserId));

                var userCollections = await _collectionService.GetCollectionsByUserIdAsync(int.Parse(UserId));
                var createdCol = userCollections.FirstOrDefault(c => c.Name == newCollectionName.Trim());

                if (createdCol != null) finalCollectionId = createdCol.Id.Value;
            }

            // Додавання сету в колекцію
            if (setId != 0 && finalCollectionId != 0)
            {
                await _setService.AddSetToCollectionAsync(setId, finalCollectionId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}