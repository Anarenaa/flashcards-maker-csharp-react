using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Repositories.Interfaces;

namespace App.Attributes
{
    public class SetOwnerAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var setId = context.RouteData.Values["id"]?.ToString();

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (string.IsNullOrEmpty(setId) || !int.TryParse(setId, out int id))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var unitOfWork = httpContext.RequestServices.GetService<IUnitOfWork>();
            if (unitOfWork == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var set = await unitOfWork.Sets.GetByIdAsync(id);
            if (set == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (set.UserId != userId)
            {
                context.Result = new ForbidResult();
                return;
            }

            await Task.CompletedTask;
        }
    }
}