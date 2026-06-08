using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;

namespace App.Controllers
{
    [Authorize(Roles = "User")]
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

        public async Task<IActionResult> Index()
        {
            var collections = (await _collectionService.GetCollectionsByUserIdAsync(UserId)).ToList();

            return View(collections);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return RedirectToAction(nameof(Index));

            var collectionDetail = await _collectionService.GetCollectionByIdAsync(id, UserId);

            if (collectionDetail == null)
            {
                return NotFound();
            }
            return View(collectionDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CollectionDTO collectionDto)
        {
            if (ModelState.IsValid)
            {
                await _collectionService.CreateCollectionAsync(collectionDto, UserId);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CollectionDTO collectionDto)
        {
            if (ModelState.IsValid && collectionDto.Id.HasValue)
            {
                await _collectionService.UpdateCollectionAsync(collectionDto);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _collectionService.DeleteCollectionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSet(int setId, int collectionId)
        {
            try
            {
                await _setService.RemoveSetFromCollectionAsync(setId, collectionId);
            }
            catch (Exception) { /* обробка помилок */ }

            return RedirectToAction("Details", new { id = collectionId });
        }
    }
}