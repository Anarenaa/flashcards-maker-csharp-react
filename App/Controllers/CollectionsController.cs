using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;

namespace App.Controllers
{
    public class CollectionsController : Controller
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
            var collections = (await _collectionService.GetCollectionsByUserIdAsync()).ToList();

            foreach (var item in collections)
            {
                if (item.Id.HasValue)
                {
                    var setsInCollection = await _unitOfWork.Sets.GetAllAsync(
                        filter: s => s.Collections.Any(c => c.Id == item.Id.Value)
                    );
                    item.SetsCount = setsInCollection.Count();

                    if (item.CreatedAt == default) item.CreatedAt = DateTime.Now;
                    if (item.LastUpdatedAt == default) item.LastUpdatedAt = DateTime.Now;
                }
            }

            return View(collections);
        }

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

        [HttpPost]
        public async Task<IActionResult> Create(CollectionDTO collectionDto)
        {
            if (ModelState.IsValid)
            {
                await _collectionService.CreateCollectionAsync(collectionDto);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _collectionService.DeleteCollectionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSet(int setId, int collectionId)
        {
            try
            {
              
                await _setService.RemoveSetFromCollectionAsync(setId, collectionId);
            }
            catch (Exception)
            {
               
            }

            return RedirectToAction("Details", new { id = collectionId });
        }
    }
}