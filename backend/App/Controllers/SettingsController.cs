using System.Threading.Tasks;
using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services; 

namespace App.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserService _userService;

        public SettingsController(UserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Профіль";
            return View();
        }
    }
}