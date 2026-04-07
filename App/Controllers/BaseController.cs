using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class BaseController : Controller
    {
        protected int UserId
        {
            get
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userIdStr, out int userId) ? userId : 0;
            }
        }
    }
}
