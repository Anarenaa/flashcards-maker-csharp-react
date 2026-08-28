using System.Security.Claims;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailService emailService,
            IConfiguration configuration
        )
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Користувач з таким email вже існує");
                return BadRequest(ModelState);
            }

            var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserName != null)
            {
                ModelState.AddModelError("UserName", "Користувач з таким іменем вже існує");
                return BadRequest(ModelState);
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
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                    return Ok(new { message = "Реєстрація та вхід успішні", username = dto.UserName });
                }
                catch
                {
                    return Ok(new { message = "Користувача створено, але не вдалося авторитаризувати. Будь ласка, увійдіть." });
                }
            }

            foreach (var error in result.Errors)
            {
                if (error.Code.Contains("Email"))
                    ModelState.AddModelError("Email", error.Description);
                else if (error.Code.Contains("UserName") || error.Code.Contains("User"))
                    ModelState.AddModelError("UserName", error.Description);
                else if (error.Code.Contains("Password"))
                    ModelState.AddModelError("Password", error.Description);
                else
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, [FromQuery] string? returnUrl = null) //returnUrl is the link that user might have searched without authorization
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); //return errors array from data.annotation validation in dto
            }

            var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";

            try
            {
                var token = await _authService.LoginAsync(dto);
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new
                {
                    message = "Вхід успішний",
                    user = dto.UserNameOrEmail,
                    userToken = token,
                    redirectTo = redirectUrl
                });
            }
            catch (UnauthorizedAccessException)
            {
                var user = await _userManager.FindByEmailAsync(dto.UserNameOrEmail)
                           ?? await _userManager.FindByNameAsync(dto.UserNameOrEmail);
                if (user == null)
                {
                    ModelState.AddModelError("UserNameOrEmail", "Користувача з таким логіном або email не знайдено");
                }
                else
                {
                    ModelState.AddModelError("Password", "Неправильний пароль");
                }

                return BadRequest(ModelState);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };

            Response.Cookies.Delete("AuthToken", cookieOptions);
            Response.Cookies.Delete(".AspNetCore.Identity.Application", cookieOptions);
            Response.Cookies.Delete(".AspNetCore.Identity.External", cookieOptions);

            // Очищаємо серверні сесії Identity
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Вихід успішний" });
        }
        
        [HttpGet("me")]
        [Authorize] 
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
            {
                throw new KeyNotFoundException("Користувача не знайдено");
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(true);
        }

        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { returnUrl });

            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse([FromQuery] string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return this.RedirectToFrontend("/login?error=google_failed");
            }

            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var avatar = result.Principal.FindFirstValue("picture")
                         ?? result.Principal.FindFirstValue("image");

            try
            {
                var token = await _authService.ExternalLoginAsync(email!, name!, avatar!, googleId!);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                var destination = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
                return this.RedirectToFrontend(destination);
            }
            catch (Exception)
            {
                return this.RedirectToFrontend("/login?error=server_error");
            }
        }

        // Reset Password ----------------------------
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Користувача з таким email не знайдено");
                return BadRequest(ModelState);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var frontendBaseUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";

            var encodedToken = Uri.EscapeDataString(resetToken);
            var resetLink = $"{frontendBaseUrl}/change-password?email={model.Email}&token={encodedToken}";

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ResetPasswordTemplate.html");
            string emailBody;

            if (System.IO.File.Exists(templatePath))
            {
                emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                emailBody = emailBody.Replace("{{ResetLink}}", resetLink);
            }
            else
            {
                emailBody = $@"
                    <h2>Скидання пароля для Flashcards Maker</h2>
                    <p>Привіт! Ми отримали запит на відновлення доступу до вашого акаунта.</p>
                    <p>Щоб створити новий пароль, перейдіть за посиланням (воно дійсне 24 години):</p>
                    <p><a href='{resetLink}'>{resetLink}</a></p>
                    <br>
                    <p>Якщо ви не робили цього запиту, просто проігноруйте цей лист.</p>";
            }

            var subject = "Скидання пароля 🗂️";

            await _emailService.SendEmailAsync(model.Email, subject, emailBody);

            return Ok(new { message = "Лист для скидання пароля відправлено" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.Token = Uri.UnescapeDataString(model.Token);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("global", "Користувача з таким email не знайдено");
                return BadRequest(ModelState);
            }

            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.NewPassword);

            if (verificationResult == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("NewPassword", "Новий пароль не може бути таким самим, як старий");
                return BadRequest(ModelState);
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("global", error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok(new { message = "Пароль успішно змінено" });
        }
    }
}
