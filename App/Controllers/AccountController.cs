using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System.Security.Claims;

namespace App.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public AccountController(
            IAuthService authService,
            UserManager<User> userManager,
            UserService userService,
            SignInManager<User> signInManager,
            EmailService emailService
        )
        {
            _authService = authService;
            _userManager = userManager;
            _userService = userService;
            _signInManager = signInManager;
            _emailService = emailService;
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
                    SameSite = SameSiteMode.Lax,
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
                    SameSite = SameSiteMode.Lax,
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
        [HttpPost("update-general-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGeneralProfile(string? userName, IFormFile? avatarFile)
        {
            try
            {

                await _userService.UpdateUserProfileAsync(UserId, userName, null, avatarFile);

                TempData["SuccessMessage"] = "Профіль успішно оновлено!";
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index", "Settings");
        }
        [Authorize]
        [HttpPost("update-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string? userName, IFormFile? avatarFile)
        {
            try
            {

                await _userService.UpdateUserProfileAsync(UserId, userName, null, avatarFile);

                TempData["SuccessMessage"] = "Профіль успішно оновлено!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Settings");
        }
        [Authorize]
        [HttpPost("delete-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                await _userService.DeleteUserAsync(UserId);
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
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete(".AspNetCore.Identity.Application");
            Response.Cookies.Delete(".AspNetCore.Identity.External");

            await _signInManager.SignOutAsync();

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача з таким email не знайдено");
                return View(model);
            } 
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ChangePassword", "Account", new { email=model.Email, token = resetToken }, Request.Scheme);
            var subject = "Скидання пароля ";
            var body = $"Щоб скинути пароль, натисніть на посилання: <a href='{resetLink}'>Скинути пароль</a>";
            await _emailService.SendEmailAsync(model.Email, subject, body);
            return RedirectToAction("EmailSent", "Account");
        }
        [HttpGet]
        public IActionResult ChangePassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            var model = new ChangePasswordDTO
            {
                Email = email,
                Token = token,
                NewPassword = "",
                ConfirmPassword = ""
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Користувача з таким email не знайдено");
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача з таким email не знайдено");
                return View(model);
            }
            var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        public IActionResult EmailSent()
        {
            return View();
        }
    }
}
