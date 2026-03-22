using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
