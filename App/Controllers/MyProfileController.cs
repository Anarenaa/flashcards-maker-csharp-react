using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using System.Threading.Tasks;

namespace App.Controllers
{
    public class MyProfileController : Controller
    {
        private readonly UserService _userService;

        public MyProfileController(UserService userService)
        {
            _userService = userService;
        }

        // Головна сторінка профілю: /MyProfile
        public async Task<IActionResult> Index()
        {
            // СИМУЛЯЦІЯ: Припустимо, ID поточного користувача = 1
            // Коли додамо логін, будемо брати через User.FindFirstValue
            int currentUserId = 1;

            try
            {
                var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
                return View(userDto);
            }
            catch
            {
                // Якщо користувача не знайдено, повертаємо на головну
                return RedirectToAction("Index", "Main");
            }
        }
    }
}