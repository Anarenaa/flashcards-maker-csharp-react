using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
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
        private readonly UserService _userService;
        private readonly IEmailService _emailService;

        public AuthController(
            IAuthService authService,
            UserManager<User> userManager,
            UserService userService,
            SignInManager<User> signInManager,
            IEmailService emailService
        )
        {
            _authService = authService;
            _userManager = userManager;
            _userService = userService;
            _signInManager = signInManager;
            _emailService = emailService;
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
    }
}
