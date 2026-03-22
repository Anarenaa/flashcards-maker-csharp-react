using Microsoft.AspNetCore.Mvc;
using Services;

namespace App.Controllers
{
    public class MySetsController : Controller
    {
        private readonly SetService _setService;
        public MySetsController(SetService setService)
        {
            _setService = setService;
        }
        
       
        public async Task<IActionResult> Index()
        {
            var sets = await _setService.GetAllSetsAsync([]);

            return View(sets);
        }
    }

}
