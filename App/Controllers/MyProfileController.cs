using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using Repositories.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Authorize]
    public class MyProfileController : Controller
    {
        private readonly UserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public MyProfileController(UserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
            var userSets = await _unitOfWork.Sets.GetAllAsync(s => s.UserId == currentUserId);
            ViewBag.SetsCount = userSets.Count();

            return View(userDto);
        }
    }
}