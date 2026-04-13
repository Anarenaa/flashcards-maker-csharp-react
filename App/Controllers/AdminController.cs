using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Interfaces;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;

    // Статичні змінні для реалістичної статистики в реальному часі
    private static int _sentEmailsCount = 0;
    private static string _lastActionTime = "Ще не було";

    public AdminController(UserService userService, UserManager<User> userManager, IEmailService emailService)
    {
        _userService = userService;
        _userManager = userManager;
        _emailService = emailService;
    }

    // --- 1. КОРИСТУВАЧІ ---
    public async Task<IActionResult> Users(string searchTerm)
    {
        var users = await _userService.GetAllUsersAsync(null, searchTerm);

        // Перевірка на AJAX запит для "живого пошуку"
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

    // --- 2. ЗАБЛОКОВАНІ ---
    public async Task<IActionResult> Blocked()
    {
        var blockedUsers = _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .ToList();
        return View(blockedUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, shouldBlock ? DateTimeOffset.MaxValue : null);

            // Оновлюємо статистику останньої дії
            _lastActionTime = DateTime.Now.ToString("HH:mm");
        }
        return RedirectToAction(shouldBlock ? "Users" : "Blocked");
    }

    // --- 3. СКАРГИ / REPORTS ---
    public IActionResult Reports(string? targetEmail)
    {
        ViewBag.TargetEmail = targetEmail;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendWarning(string email, string subject, string message)
    {
        await _emailService.SendEmailAsync(email, subject, message);

        _sentEmailsCount++;
        _lastActionTime = DateTime.Now.ToString("HH:mm");

        TempData["Success"] = $"Лист успішно надіслано на {email}";
        return RedirectToAction("Reports");
    }

    // --- 4. НАЛАШТУВАННЯ ---
    public IActionResult Settings() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewAdmin(string email, string password)
    {
        var result = await _userService.CreateAdminAsync(email, password);
        if (result.Succeeded) TempData["Success"] = "Нового адміністратора створено!";
        else TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
        return RedirectToAction("Settings");
    }

    // --- 5. API ДЛЯ ЖИВОГО ПОШУКУ (Email Suggestions) ---
    [HttpGet]
    public async Task<JsonResult> GetEmailSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2) return Json(new List<object>());

        var users = await _userService.GetAllUsersAsync(null, query);
        var suggestions = users.Select(u => new { u.Email, u.UserName }).Take(5);
        return Json(suggestions);
    }

    // --- 6. API ДЛЯ РЕАЛЬНОГО ЧАСУ (Live Stats) ---
    [HttpGet]
    public async Task<JsonResult> GetRealTimeStats()
    {
        var today = DateTime.Today;

        int newUsersToday = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= today);

        int blockedCount = await _userManager.Users
            .CountAsync(u => u.LockoutEnd > DateTimeOffset.UtcNow);

        return Json(new
        {
            emailsToday = _sentEmailsCount,
            newUsers = newUsersToday,
            blockedTotal = blockedCount,
            lastAction = _lastActionTime
        });
    }
}