using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly UserService _userService;
        private readonly SignInManager<User> _signInManager;
        public UsersController(UserService userService, SignInManager<User> signInManager)
        {
            _userService = userService;
            _signInManager = signInManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetUserAsync(UserId, UserId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUserProfile([FromForm] UpdateUserDto dto)
        {
            await _userService.UpdateUserProfileAsync(UserId, dto.UserName, dto.AvatarFile, dto.RemoveAvatar);
            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUser()
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

            await _signInManager.SignOutAsync();
            await _userService.DeleteUserAsync(UserId);
            return NoContent();
        }

        [HttpPost("switch-my-publicity")]
        public async Task<IActionResult> SwitchMyProfilePublicity()
        {
            await _userService.SwitchProfilePublicityAsync(UserId);
            return NoContent();
        }

        [HttpPost("make-my-sets-private")]
        public async Task<IActionResult> MakeMySetsPrivate()
        {
            await _userService.MakeUserSetsPrivate(UserId);
            return NoContent();
        }
    }
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public bool RemoveAvatar { get; set; }
    }
}
