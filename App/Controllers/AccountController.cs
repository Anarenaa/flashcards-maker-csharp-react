using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserService _userService;

        public AccountController(IAuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.RegisterAsync(dto);
            
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View(dto);
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(dto);
            }

            try
            {
                var token = await _authService.LoginAsync(dto);
                
                // Зберігаємо JWT токен в cookie для Razor
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                // Додаємо токен в ViewBag для JavaScript
                ViewBag.AuthToken = token;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException)
            {
                ModelState.AddModelError(string.Empty, "Неправильний email або пароль");
                ViewData["ReturnUrl"] = returnUrl;
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet("my-profile")]
        public async Task<IActionResult> MyProfile()
        {
            if (!int.TryParse(UserId, out int currentUserId))
            {
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login");
            }
            try
            {
                var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
                return View(userDto);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPost("update-avatar")]
        [ValidateAntiForgeryToken] // Захист від підробки запитів з інших сайтів
        public async Task<IActionResult> UpdateAvatar(string newPath)
        {
            if (string.IsNullOrWhiteSpace(newPath))
            {
                TempData["ErrorMessage"] = "Посилання на фото не може бути порожнім.";
                return RedirectToAction("MyProfile");
            }

            try
            {
                await _userService.UpdateMyProfileAvatarAsync(int.Parse(UserId), newPath);

                TempData["SuccessMessage"] = "Аватар успішно оновлено!";
            }
            catch (NotFoundException)
            {
                return NotFound("Користувача не знайдено в системі.");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Сталася помилка при оновленні: " + ex.Message;
            }

            return RedirectToAction("MyProfile");
        }
        [Authorize]
        [HttpPost("delete-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                if (!int.TryParse(UserId, out int currentUserId))
                {
                    return RedirectToAction("Login");
                }

                await _userService.DeleteUserAsync(currentUserId);
                Response.Cookies.Delete("AuthToken");

                TempData["SuccessMessage"] = "Ваш профіль було успішно видалено.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при видаленні профілю: " + ex.Message;
                return RedirectToAction("MyProfile");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            
            return RedirectToAction("Login");
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
