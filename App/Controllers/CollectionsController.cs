using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers
{
    public class CollectionsController : BaseController
    {
        private readonly CollectionService _collectionService;
        private readonly SetService _setService;
        private readonly IUnitOfWork _unitOfWork;

        public CollectionsController(
            CollectionService collectionService,
            SetService setService,
            IUnitOfWork unitOfWork)
        {
            _collectionService = collectionService;
            _setService = setService;
            _unitOfWork = unitOfWork;
        }

        // 1. Список колекцій
        public async Task<IActionResult> Index()
        {
            var collections = (await _collectionService.GetCollectionsByUserIdAsync(int.Parse(UserId))).ToList();

            foreach (var item in collections)
            {
                if (item.Id.HasValue)
                {
                    var setsInCollection = await _unitOfWork.Sets.GetAllAsync(
                        filter: s => s.Collections.Any(c => c.Id == item.Id.Value)
                    );
                    item.SetsCount = setsInCollection.Count();
                }
            }

            return View(collections);
        }

        // 2. Деталі колекції
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var collectionDetail = await _collectionService.GetCollectionByIdAsync(id);
                return View(collectionDetail);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. Створення
        [HttpPost]
        public async Task<IActionResult> Create(CollectionDTO collectionDto)
        {
            if (ModelState.IsValid)
            {
                await _collectionService.CreateCollectionAsync(collectionDto, int.Parse(UserId));
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. НОВИЙ МЕТОД: Редагування (Update)
        [HttpPost]
        public async Task<IActionResult> Update(CollectionDTO collectionDto)
        {
            if (ModelState.IsValid && collectionDto.Id.HasValue)
            {
                await _collectionService.UpdateCollectionAsync(collectionDto);
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. Видалення
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _collectionService.DeleteCollectionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 6. Видалення сету з папки
        [HttpPost]
        public async Task<IActionResult> RemoveSet(int setId, int collectionId)
        {
            try
            {
                await _setService.RemoveSetFromCollectionAsync(setId, collectionId);

                var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
                if (collection != null)
                {
                    collection.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception) { /* обробка помилок */ }

            return RedirectToAction("Details", new { id = collectionId });
        }
    }
}