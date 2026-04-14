using App.Controllers;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly ReportService _reportService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SetService _setService;

    private static int _sentEmailsCount = 0;
    private static string _lastActionTime = "Ще не було";

    public AdminController(
        UserService userService,
        UserManager<User> userManager,
        ReportService reportService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        SetService setService)
    {
        _userService = userService;
        _userManager = userManager;
        _reportService = reportService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _setService = setService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Users));

    // --- 1. КЕРУВАННЯ КОРИСТУВАЧАМИ ---
    public async Task<IActionResult> Users(string searchTerm)
    {
        var allUsers = await _userService.GetAllUsersAsync(UserId, null, searchTerm);

        ViewBag.WarningCounts = await _reportService.GetReportsCountPerUserAsync();
        ViewBag.BlockedUserIds = await _userService.GetBlockedUserIdsAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("UserTableRows", allUsers);
        }

        return View(allUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock, int? days)
    {
        await _userService.ToggleUserBlockAsync(id, shouldBlock, days);

        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = shouldBlock ? "Статус користувача змінено" : "Користувача розблоковано";

        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> UserDetails(int id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        if (profile == null) return NotFound();
        return View(profile);
    }

    // --- 2. ЦЕНТР МОДЕРАЦІЇ (СКАРГИ) ---
    public async Task<IActionResult> Reports(int? userId)
    {
        if (userId.HasValue)
        {
            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user != null) ViewBag.TargetEmail = user.Email;
        }

        ViewBag.AdminWarnings = await _reportService.GetAdminWarningsAsync();
        ViewBag.UserComplaints = await _reportService.GetUserComplaintsAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendWarning(string email, string subject, string message)
    {
        var targetUser = await _userManager.FindByEmailAsync(email);
        await _emailService.SendEmailAsync(email, subject, message);

        if (targetUser != null)
        {
            var systemReport = new CreateReportDTO
            {
                ReportedUserId = targetUser.Id,
                Reason = ReportReason.Other,
                CustomReason = "Попередження: " + subject,
            };
            await _reportService.CreateReportAsync(systemReport, UserId);
        }

        _sentEmailsCount++;
        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = $"Лист надіслано на {email}";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReport(int id)
    {
        try
        {
            await _reportService.DeleteReportAsync(id);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            Console.WriteLine($"Помилка при видаленні скарги: {ex.Message}");
            return RedirectToAction(nameof(Reports));
        }
        _lastActionTime = DateTime.Now.ToString("HH:mm");
        return RedirectToAction(nameof(Reports));
    }

    // --- 3. ДЕТАЛІ СЕТУ ---
    public async Task<IActionResult> SetDetails(int id)
    {
        var setDetails = await _setService.GetSetByIdAsync(id);
        if (setDetails == null) return NotFound();
        return View(setDetails);
    }

    // --- 4. API ТА СТАТИСТИКА ---
    [HttpGet]
    public async Task<JsonResult> GetEmailSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2) return Json(new List<object>());
        var users = await _userService.GetAllUsersAsync(null, query);
        return Json(users.Select(u => new { u.Email, u.UserName }).Take(5));
    }

    [HttpGet]
    public async Task<JsonResult> GetRealTimeStats()
    {
        return Json(new
        {
            emailsToday = _sentEmailsCount,
            newUsers = await _userManager.Users.CountAsync(u => u.CreatedAt >= DateTime.Today),
            blockedTotal = await _userManager.Users.CountAsync(u => u.LockoutEnd > DateTimeOffset.UtcNow),
            totalUsers = await _userManager.Users.CountAsync(),
            lastAction = _lastActionTime
        });
    }

    public IActionResult Settings() => View();
}