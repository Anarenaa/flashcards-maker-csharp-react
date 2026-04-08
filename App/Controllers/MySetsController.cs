using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Authorize(Roles = "User")]
    public class MySetsController : BaseController
    {
        private readonly SetService _setService;
        private readonly CollectionService _collectionService; 

        public MySetsController(SetService setService, CollectionService collectionService)
        {
            _setService = setService;
            _collectionService = collectionService;
        }

        public async Task<IActionResult> Index(string? searchText, string sortOrder = "newest")
        {
            var sets = await _setService.GetAllUserSetsAsync(UserId, null, searchText);

            sets = sortOrder switch
            {
                "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
                "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
                _ => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            };

            ViewData["CurrentFilter"] = searchText;
            ViewData["CurrentSort"] = sortOrder;

            ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(UserId);

            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SetDTO setDto)
        {
            if (ModelState.IsValid)
            {
                setDto.CreatedAt = DateTime.Now;
                setDto.LastUpdatedAt = DateTime.Now;

                await _setService.AddSetAsync(setDto, UserId);
                return RedirectToAction(nameof(Index));
            }
            return await Index(null);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SetDTO setDto)
        {
            if (ModelState.IsValid)
            {
                if (!await _setService.IsSetMine(setDto.Id.Value, UserId))
                {
                    return Forbid();
                }

                setDto.LastUpdatedAt = DateTime.Now;

                await _setService.UpdateSetAsync(setDto);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!await _setService.IsSetMine(id, UserId))
            {
                return Forbid();
            }

            await _setService.DeleteSetAsync(id);
            return RedirectToAction(nameof(Index));
        }

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