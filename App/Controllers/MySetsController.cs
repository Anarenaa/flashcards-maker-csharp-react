using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System.Text.Json;

namespace App.Controllers;

[Authorize(Roles = "User")]
public class MySetsController(
    SetService setService,
    CollectionService collectionService,
    CategoryService categoryService,
    FlashcardService flashcardService,
    IPracticeService practiceService) : BaseController
{
    public async Task<IActionResult> Index(string? searchText, int? categoryId, string sortOrder = "newest")
    {
        List<int>? categoryIds = categoryId.HasValue ? [categoryId.Value] : null;

        var sets = await setService.GetAllUserSetsAsync(UserId, categoryIds, searchText);

        foreach (var set in sets)
        {
            if (set.Id.HasValue)
            {
                var progressInfo = await practiceService.GetSetProgressAsync(set.Id.Value, UserId);

                set.Progress = (int)Math.Round(progressInfo.OverallProgress * 100);
            }
        }

        sets = sortOrder switch
        {
            "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
            "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            _ => sets.OrderByDescending(s => s.CreatedAt).ToList()
        };

        // 4. Дані для фільтрів та навігації
        ViewBag.Categories = await categoryService.GetAllCategoriesAsync();
        ViewBag.UserCollections = await collectionService.GetCollectionsByUserIdAsync(UserId);

        ViewData["CurrentFilter"] = searchText;
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentSort"] = sortOrder;

        return View(sets);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SetCreateDTO setDto, string? GeneratedCardsJson)
    {
        if (ModelState.IsValid)
        {
            var createdSet = await setService.AddSetAsync(setDto, UserId);
            return RedirectToAction("Index", "Details", new { id = createdSet.Id });
        }

        return await Index(null, null, "newest");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SetCreateDTO setDto)
    {
        if (!ModelState.IsValid || id == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await setService.IsSetMine(id, UserId))
        {
            return Forbid();
        }

        await setService.UpdateSetAsync(id, setDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await setService.IsSetMine(id, UserId))
        {
            return Forbid();
        }

        await setService.DeleteSetAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCollection(int setId, int collectionId)
    {
        if (setId != 0 && collectionId != 0)
        {
            await setService.AddSetToCollectionAsync(setId, collectionId);
        }

        return RedirectToAction(nameof(Index));
    }
}