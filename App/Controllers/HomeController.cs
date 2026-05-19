using System.Diagnostics;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (Request.Cookies.ContainsKey("AuthToken") || User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Main");
            }
            return View();
        }

        [AllowAnonymous]
        public IActionResult ServiceUnavailable()
        {
            return View("/Views/Shareds/ServiceUnavailable.cshtml");
        }

        [AllowAnonymous]
        [Route("Home/NotFoundPage/{id?}")]
        public IActionResult NotFoundPage(int? id)
        {
            return View("/Views/Shareds/NotFoundPage.cshtml");
        }

        [AllowAnonymous]
        [Route("access-denied")]
        public IActionResult AccessDenied()
        {
            return View("/Views/Shareds/AccessDenied.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}