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

        public async Task<IActionResult> Index(string? searchText)
        {
           
            var sets = await _setService.GetAllUserSetsAsync(null, null, searchText);

            ViewData["CurrentFilter"] = searchText;

            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SetDTO setDto)
        {
            if (!ModelState.IsValid)
            {
                var allSets = await _setService.GetAllUserSetsAsync(null, null, null);
                return View("Index", allSets);
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
    }
}