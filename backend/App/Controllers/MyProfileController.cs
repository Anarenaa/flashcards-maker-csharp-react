using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Services.Practice;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace App.Controllers
{
    public class MyProfileController : BaseController
    {
        private readonly UserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IProgressService _progressService;

        public MyProfileController(UserService userService, UserManager<User> userManager, IProgressService progressService)
        {
            _userService = userService;
            _userManager = userManager;
            _progressService = progressService;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userProfile = await _userService.GetMyPrivateProfileAsync(userId);

            var progress = await _progressService.GetUserProgressAsync(userId);

            ViewBag.IsPrivate = await _userService.IsProfilePrivate(userId);
            ViewBag.UserProgress = progress;

            return View(userProfile);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicity()
        {
            await _userService.SwitchProfilePublicity(UserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "User")]
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

        [HttpGet("MyProfile/UserProfile/{username}")]
        [Authorize]
        public async Task<IActionResult> UserProfile(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null) return NotFound();

                bool isPrivate = await _userService.IsProfilePrivate(user.Id);
                ViewBag.IsPrivate = isPrivate;

                var publicProfile = await _userService.GetUserProfileAsync(user.Id);

                var progress = await _progressService.GetUserProgressAsync(user.Id);
                ViewBag.UserProgress = progress;

                return View(publicProfile);
            }
            catch
            {
                return RedirectToAction("Index", "Main");
            }
        }
    }
}