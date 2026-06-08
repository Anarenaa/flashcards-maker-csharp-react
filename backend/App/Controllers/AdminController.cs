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
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly ReportService _reportService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SetService _setService;
    private readonly CategoryService _categoryService;

    private static string _lastActionTime = "Ще не було";

    public AdminController(
        UserService userService,
        UserManager<User> userManager,
        ReportService reportService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        SetService setService,
        CategoryService categoryService)
    {
        _userService = userService;
        _userManager = userManager;
        _reportService = reportService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _setService = setService;
        _categoryService = categoryService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Users));

    // --- 1. КЕРУВАННЯ КОРИСТУВАЧАМИ ---
    public async Task<IActionResult> Users(string searchTerm)
    {
        var allUsers = await _userService.GetAllUsersAsync(UserId, null, searchTerm);

        ViewBag.WarningCounts = await _reportService.GetReportsCountPerUserAsync();
        ViewBag.BlockedUserIds = await _userService.GetBlockedUserIdsAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("UserTableRows", allUsers);

        return View(allUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock, int? days = 30)
    {
        await _userService.ToggleUserBlockAsync(id, shouldBlock, days);

        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = shouldBlock ? $"Користувача заблоковано на {days} днів" : "Користувача розблоковано";

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id, string reason = "Порушення правил платформи")
    {
        try
        {
            await _userService.DeleteUserByAdminAsync(id, reason);
            _lastActionTime = DateTime.Now.ToString("HH:mm");
            TempData["Success"] = "Акаунт видалено назавжди. Користувачу надіслано лист.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Помилка при видаленні: " + ex.Message;
        }
        return RedirectToAction(nameof(Users));
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
                CustomReason = "Попередження: " + subject + " | " + message,
            };
            await _reportService.CreateReportAsync(systemReport, UserId);
        }

        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = $"Лист надіслано на {email}";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReport(int id)
    {
        await _reportService.DeleteReportAsync(id);
        _lastActionTime = DateTime.Now.ToString("HH:mm");
        return RedirectToAction(nameof(Reports));
    }

    // --- 3. ДЕТАЛІ СЕТУ ТА ЮЗЕРА ---
    public async Task<IActionResult> UserDetails(int id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        return profile == null ? NotFound() : View(profile);
    }

    public async Task<IActionResult> SetDetails(int id)
    {
        var setDetails = await _setService.GetSetByIdAsync(id);
        return setDetails == null ? NotFound() : View(setDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSetByAdmin(int setId)
    {
        var set = await _unitOfWork.Sets.GetByIdAsync(setId);
        if (set != null)
        {
            _unitOfWork.Sets.Delete(set);
            await _unitOfWork.SaveChangesAsync();
            _lastActionTime = DateTime.Now.ToString("HH:mm");
            TempData["Success"] = "Сет видалено";
        }
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCardByAdmin(int cardId, int setId, string term, string definition)
    {
        var card = await _unitOfWork.Flashcards.GetByIdAsync(cardId);
        if (card != null)
        {
            card.Term = term;
            card.Definition = definition;
            await _unitOfWork.SaveChangesAsync();
            _lastActionTime = DateTime.Now.ToString("HH:mm");
            TempData["Success"] = "Картку оновлено";
        }
        return RedirectToAction(nameof(SetDetails), new { id = setId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCardByAdmin(int cardId, int setId)
    {
        var card = await _unitOfWork.Flashcards.GetByIdAsync(cardId);
        if (card != null)
        {
            _unitOfWork.Flashcards.Delete(card);
            await _unitOfWork.SaveChangesAsync();
            _lastActionTime = DateTime.Now.ToString("HH:mm");
        }
        return RedirectToAction(nameof(SetDetails), new { id = setId });
    }

    // --- 4. КЕРУВАННЯ КАТЕГОРІЯМИ ---
    public async Task<IActionResult> Categories()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return View(categories.OrderBy(c => c.Name).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _categoryService.CreateCategoryAsync(new CategoryDTO { Name = name.Trim() });
            TempData["Success"] = "Категорію створено";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(int id, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _categoryService.UpdateCategoryAsync(new CategoryDTO { Id = id, Name = name.Trim() });
            TempData["Success"] = "Категорію оновлено";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return RedirectToAction(nameof(Categories));
    }

    // --- 5. API ТА СТАТИСТИКА (LIVE) ---
    [HttpGet]
    public async Task<JsonResult> GetRealTimeStats()
    {
        try
        {
            var today = DateTime.Today;
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers.Select(a => a.Id).ToHashSet();

            var reportsToday = await _unitOfWork.Reports.GetAllAsync(r => r.CreatedAt >= today);

            int emailsSent = reportsToday.Count(r => adminIds.Contains(r.ReporterId));
            int userComplaints = reportsToday.Count(r => !adminIds.Contains(r.ReporterId));
            int newUsers = await _userManager.Users.CountAsync(u => u.CreatedAt >= today);
            int total = await _userManager.Users.CountAsync();
            int blocked = await _userManager.Users.CountAsync(u => u.LockoutEnd > DateTimeOffset.UtcNow);

            return Json(new
            {
                emailsToday = emailsSent,
                userComplaintsToday = userComplaints,
                newUsers = newUsers,
                totalUsers = total,
                blockedTotal = blocked,
                lastAction = _lastActionTime
            });
        }
        catch
        {
            return Json(new { error = "Stats error" });
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetEmailSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2) return Json(new List<object>());
        var users = await _userService.GetAllUsersAsync(UserId, null, query);
        return Json(users.Select(u => new { u.Email, u.UserName }).Take(5));
    }

    public IActionResult Settings() => View();
}