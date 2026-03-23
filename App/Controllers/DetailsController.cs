using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class DetailsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
