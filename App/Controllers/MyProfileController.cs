using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    [Authorize]
    public class MyProfileController : BaseController
    {
        private readonly UserService _userService;
        private readonly UserManager<User> _userManager;
        public MyProfileController(UserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userDto = await _userService.GetMyPrivateProfileAsync(UserId);
            return View(userDto);
        }

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
      
        [HttpGet("MyProfile/UserProfile/{username}")]
        public async Task<IActionResult> UserProfile(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null) return NotFound();
                var publicProfile = await _userService.GetUserProfileAsync(user.Id);

                return View(publicProfile);
            }
            catch
            {
                return RedirectToAction("Index", "Main");
            }
        }
    }
}