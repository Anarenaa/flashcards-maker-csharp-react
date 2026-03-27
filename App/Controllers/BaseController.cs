using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class BaseController : Controller
    {
        protected string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}
