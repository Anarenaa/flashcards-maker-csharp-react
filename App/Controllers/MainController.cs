using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading.Tasks;

namespace App.Controllers
{
    public class MainController : Controller
    {
        private readonly SetService _setService;
        public MainController(SetService setService)
        {
            _setService = setService;
        }
        public async Task<IActionResult> Index(string? searchText)
        {    
            var sets = await _setService.GetAllSetsAsync(null, searchText);
            ViewData["CurrentFilter"] = searchText;
            return View(sets);
        }
    }
}