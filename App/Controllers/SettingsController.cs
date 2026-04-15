using System.Threading.Tasks;
using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services; 

namespace App.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserService _userService;

        public SettingsController(UserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            int currentUserId = 1;

            try
            {
                var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
                return View(userDto);
            }
            catch
            {
                var fallbackModel = new PrivateUserDTO
                {
                    UserName = "Користувач",
                    Email = "email@example.com",
                    AvatarUrl = ""
				};
                return View(fallbackModel);
            }
        }

        public IActionResult Settings()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Профіль";
            return View();
        }
    }
}