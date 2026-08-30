using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly UserService _userService;
        public UsersController(UserService userService)
        {
            _userService = userService;
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
    }
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public bool RemoveAvatar { get; set; }
    }
}
