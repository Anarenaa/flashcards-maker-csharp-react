using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;

namespace App.Controllers;

[Authorize(Roles = "User")]
public class MySetsController(
    SetService setService,
    CollectionService collectionService,
    CategoryService categoryService,
    FlashcardService flashcardService) : BaseController 
{
    public async Task<IActionResult> Index(string? searchText, int? categoryId, string sortOrder = "newest")
    {
        List<int>? categoryIds = categoryId.HasValue ? [categoryId.Value] : null;
        var sets = await setService.GetAllUserSetsAsync(UserId, categoryIds, searchText);

        sets = sortOrder switch
        {
            "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
            "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            _ => sets.OrderByDescending(s => s.CreatedAt).ToList()
        };

        ViewBag.Categories = await categoryService.GetAllCategoriesAsync();
        ViewBag.UserCollections = await collectionService.GetCollectionsByUserIdAsync(UserId);

        ViewData["CurrentFilter"] = searchText;
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentSort"] = sortOrder;

        return View(sets);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SetDTO setDto, string? GeneratedCardsJson)
    {
        if (ModelState.IsValid)
        {
            setDto.CreatedAt = DateTime.UtcNow;
            setDto.LastUpdatedAt = DateTime.UtcNow;

            var createdSet = await setService.AddSetAsyncWithReturn(setDto, UserId);

            if (!string.IsNullOrEmpty(GeneratedCardsJson))
            {
                try
                {
                    var cards = JsonSerializer.Deserialize<List<FlashcardDTO>>(GeneratedCardsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (cards != null && cards.Any())
                    {
                        await flashcardService.CreateFlashcardsRangeAsync(createdSet.Id, cards);
                    }
                }
                catch
                {
                }
            }

            return RedirectToAction(nameof(Index));
        }

        return await Index(null, null, "newest");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SetDTO setDto)
    {
        if (!ModelState.IsValid || !setDto.Id.HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await setService.IsSetMine(setDto.Id.Value, UserId))
        {
            return Forbid();
        }

        setDto.LastUpdatedAt = DateTime.UtcNow;
        await setService.UpdateSetAsync(setDto);

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