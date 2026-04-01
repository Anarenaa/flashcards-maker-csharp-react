using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using Repositories.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Authorize]
    public class MyProfileController : BaseController
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
            if (!int.TryParse(UserId, out int currentUserId))
            {
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login", "Account");
            }

            var userDto = await _userService.GetMyPrivateProfileAsync(currentUserId);
            var userSets = await _unitOfWork.Sets.GetAllAsync(s => s.UserId == currentUserId);
            ViewBag.SetsCount = userSets.Count();

            return View(userDto);
        }
    }
}