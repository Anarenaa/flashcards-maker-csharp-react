using Microsoft.AspNetCore.Mvc;
using Services;
using Core.DTOs;
using System.Threading.Tasks;

namespace App.Controllers
{
    public class MainController : Controller
    {
        private readonly SetService _setService;
        private readonly CollectionService _collectionService; 

        public MainController(SetService setService, CollectionService collectionService)
        {
            _setService = setService;
            _collectionService = collectionService;
        }

        public async Task<IActionResult> Index(string? searchText)
        {
          
            var sets = await _setService.GetAllSetsAsync(null, searchText);

            ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(1);

            ViewData["CurrentFilter"] = searchText;
            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCollection(int setId, int? collectionId, string? newCollectionName)
        {
            int finalCollectionId = collectionId ?? 0;

            if (!string.IsNullOrWhiteSpace(newCollectionName))
            {
                var newCol = new CollectionDTO { Name = newCollectionName.Trim() };
                await _collectionService.CreateCollectionAsync(newCol);

                var userCollections = await _collectionService.GetCollectionsByUserIdAsync(1);
                var createdCol = userCollections.FirstOrDefault(c => c.Name == newCollectionName.Trim());

                if (createdCol != null) finalCollectionId = createdCol.Id.Value;
            }

            if (setId != 0 && finalCollectionId != 0)
            {
                await _setService.AddSetToCollectionAsync(setId, finalCollectionId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}