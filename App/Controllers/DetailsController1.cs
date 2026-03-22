using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class DetailsController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
