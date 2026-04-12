using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Services;
using Services.Interfaces;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;

    public AdminController(UserService userService, UserManager<User> userManager, IEmailService emailService)
    {
        _userService = userService;
        _userManager = userManager;
        _emailService = emailService;
    }

    // 2. КОРИСТУВАЧІ
    public async Task<IActionResult> Users(string searchTerm)
    {
        var users = await _userService.GetAllUsersAsync(null, searchTerm);

        // Перевірка на AJAX запит
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("UserTableRows", users);
        }

        return View(users);
    }

    public async Task<IActionResult> UserDetails(int id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        return View(profile);
    }

    // 3. ЗАБЛОКОВАНІ
    public async Task<IActionResult> Blocked()
    {
        var blockedUsers = _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .ToList();
        return View(blockedUsers);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, shouldBlock ? DateTimeOffset.MaxValue : null);
        }
        return RedirectToAction(shouldBlock ? "Users" : "Blocked");
    }

    // 1. СКАРГИ (Надсилання Email)
    public IActionResult Reports(string? targetEmail)
    {
        ViewBag.TargetEmail = targetEmail;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendWarning(string email, string subject, string message)
    {
        await _emailService.SendEmailAsync(email, subject, message);
        TempData["Success"] = $"Лист успішно надіслано на {email}";
        return RedirectToAction("Reports");
    }

    // 4. НАЛАШТУВАННЯ
    public IActionResult Settings() => View();

    [HttpPost]
    public async Task<IActionResult> CreateNewAdmin(string email, string password)
    {
        var result = await _userService.CreateAdminAsync(email, password);
        if (result.Succeeded) TempData["Success"] = "Нового адміністратора створено!";
        else TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
        return RedirectToAction("Settings");
    }

}