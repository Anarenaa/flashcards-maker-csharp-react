using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class SettingsController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Settings()
        {
            ViewData["Title"] = "Налаштування";
            return View();
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Профіль";
            return View();
        }
    }
}
