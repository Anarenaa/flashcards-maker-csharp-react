using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static RedirectResult RedirectToFrontend(this ControllerBase controller, string path)
    {
        var config = controller.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        
        var baseUrl = config["FrontendUrl"] ?? "http://localhost:5173";
        
        var formattedPath = path.StartsWith("/") ? path : "/" + path;
        
        return controller.Redirect($"{baseUrl}{formattedPath}");
    }
}