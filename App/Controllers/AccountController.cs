using System.Security.Claims;
using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly UserService _userService;

        public AccountController(IAuthService authService, UserManager<User> userManager, UserService userService)
        {
            _authService = authService;
            _userManager = userManager;
            _userService = userService;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, string? confirm)
        {
            // Спочатку перевіряємо базову валідацію
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Потім перевіряємо підтвердження пароля
            if (dto.Password != confirm)
            {
                ModelState.AddModelError(string.Empty, "Паролі не збігаються");
                return View(dto);
            }

            // Перевірка унікальності email
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Користувач з таким email вже існує");
                return View(dto);
            }

            // Перевірка унікальності імені користувача
            var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserName != null)
            {
                ModelState.AddModelError(string.Empty, "Користувач з таким іменем вже існує");
                return View(dto);
            }

            var result = await _authService.RegisterAsync(dto);

            if (result.Succeeded)
            {
             
                var loginDto = new LoginDto
                {
                    UserNameOrEmail = dto.Email,
                    Password = dto.Password
                };

                try
                {
          
                    var token = await _authService.LoginAsync(loginDto);

                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                    return RedirectToAction("Index", "Main");
                }
                catch
                {
                    return RedirectToAction("Login");
                }
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

                ViewBag.AuthToken = token;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Main");
            }
            catch (UnauthorizedAccessException)
            {
                ModelState.AddModelError(string.Empty, "Неправильний email або пароль");
                ViewData["ReturnUrl"] = returnUrl;
                return View(dto);
            }
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            // Отримуємо дані від Google через тимчасову схему кук
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return RedirectToAction("Login");
            }

            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var avatar = result.Principal.FindFirstValue("picture")
                         ?? result.Principal.FindFirstValue("image");

            try
            {
                var token = await _authService.ExternalLoginAsync(email, name, avatar, googleId);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return RedirectToAction("Index", "Main");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return RedirectToAction("Login");
            }
        }
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
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
        [Authorize]
        [HttpPost("save-theme")]
        public async Task<IActionResult> SaveTheme(string bg, string accent, string btn)
        {
            if (!int.TryParse(UserId, out int currentUserId)) return Unauthorized();

            try
            {
                
                var themeData = $"{bg}|{accent}|{btn}";
                Response.Cookies.Append("UserTheme", themeData, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = false 
                });

                return Ok();
            }
            catch { return BadRequest(); }
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
