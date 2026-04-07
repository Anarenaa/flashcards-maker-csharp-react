using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;

namespace App.Controllers
{
    [Authorize]
    public class MyProfileController : BaseController
    {
        private readonly UserService _userService;

        public MyProfileController(UserService userService)
        {
            _userService = userService;
        }

        // Відображення профілю
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userDto = await _userService.GetMyPrivateProfileAsync(UserId);
            return View(userDto);
        }

        // Оновлення даних прямо з профілю
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string? userName, IFormFile? avatarFile)
        {
            try
            {
                await _userService.UpdateUserProfileAsync(UserId, userName, null, avatarFile);
                TempData["Success"] = "Профіль оновлено!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}