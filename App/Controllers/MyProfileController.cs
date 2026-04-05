using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Core.DTOs;

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
            if (!int.TryParse(UserId, out int currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
            return View(userDto);
        }

        // Оновлення даних прямо з профілю
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string? userName, IFormFile? avatarFile)
        {
            if (!int.TryParse(UserId, out int currentUserId)) return Unauthorized();

            try
            {
                await _userService.UpdateUserProfileAsync(currentUserId, userName, null, avatarFile);
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