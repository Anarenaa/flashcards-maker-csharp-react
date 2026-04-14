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
public class AdminController : Controller
{
    private readonly UserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SetService _setService;

    private static string _lastActionTime = "Ще не було";
    private readonly DateTimeOffset SoftDeleteMarker = new DateTimeOffset(new DateTime(2099, 1, 1));

    public AdminController(
        UserService userService,
        UserManager<User> userManager,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        SetService setService)
    {
        _userService = userService;
        _userManager = userManager;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _setService = setService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Users));

    // --- 1. КЕРУВАННЯ КОРИСТУВАЧАМИ ---
    public async Task<IActionResult> Users(string searchTerm)
    {
        var allUsers = await _userService.GetAllUsersAsync(null, searchTerm);

        var reports = await _unitOfWork.Reports.GetAllAsync();
        var warningCounts = reports
            .Where(r => r.ReportedUserId.HasValue)
            .GroupBy(r => r.ReportedUserId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var now = DateTimeOffset.UtcNow;
        var lockoutUsers = await _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > now)
            .Select(u => new { u.Id, u.LockoutEnd })
            .ToListAsync();

        ViewBag.WarningCounts = warningCounts;
        ViewBag.DeletedUserIds = lockoutUsers.Where(u => u.LockoutEnd == SoftDeleteMarker).Select(u => u.Id).ToList();
        ViewBag.BlockedUserIds = lockoutUsers.Where(u => u.LockoutEnd != SoftDeleteMarker).Select(u => u.Id).ToList();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("UserTableRows", allUsers);

        return View(allUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id, string reason = "Порушення правил")
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, SoftDeleteMarker);
            await _emailService.SendEmailAsync(user.Email, "Акаунт видалено", $"Причина: {reason}");
            _lastActionTime = DateTime.Now.ToString("HH:mm");
            TempData["Success"] = "Користувача переміщено у видалені";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            _lastActionTime = DateTime.Now.ToString("HH:mm");
            TempData["Success"] = "Акаунт відновлено";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, shouldBlock ? DateTimeOffset.MaxValue.AddYears(-20) : null);
            _lastActionTime = DateTime.Now.ToString("HH:mm");
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

        var allReports = await _unitOfWork.Reports.GetAllAsync(includeProperties: "Reporter,ReportedUser,ReportedSet");
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = adminUsers.Select(a => a.Id).ToHashSet();

        var adminWarnings = allReports.Where(r => adminIds.Contains(r.ReporterId)).ToList();
        var userComplaints = allReports.Where(r => !adminIds.Contains(r.ReporterId)).ToList();

        ViewBag.AdminWarnings = adminWarnings.OrderByDescending(x => x.CreatedAt).ToList();
        ViewBag.UserComplaints = userComplaints.OrderByDescending(x => x.CreatedAt).ToList();

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
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var report = new Report
            {
                ReporterId = adminId,
                ReportedUserId = targetUser.Id,
                Reason = ReportReason.Other,
                CustomReason = "Попередження: " + subject,
                CreatedAt = DateTime.UtcNow,
                IsResolved = true
            };
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
        }

        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = $"Лист надіслано на {email}";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(id);
        if (report != null)
        {
            _unitOfWork.Reports.Delete(report);
            await _unitOfWork.SaveChangesAsync();
            _lastActionTime = DateTime.Now.ToString("HH:mm");
        }
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

    // --- 4. API ДЛЯ МОНІТОРИНГУ (LIVE STATS) ---
    [HttpGet]
    public async Task<JsonResult> GetRealTimeStats()
    {
        try
        {
            var today = DateTime.Today;
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(a => a.Id).ToList();
            int newUsersCount = await _userManager.Users.CountAsync(u => u.CreatedAt >= today);

            // Рахуємо реальні звіти (листи) від адмінів за сьогодні
            var reportsToday = await _unitOfWork.Reports.GetAllAsync(r => r.CreatedAt >= today);
            int emailsCount = reportsToday.Count(r => adminIds.Contains(r.ReporterId));
            int complaintsFromUsers = reportsToday.Count(r => !adminIds.Contains(r.ReporterId));
            int totalUsers = await _userManager.Users.CountAsync();
            int blockedTotal = await _userManager.Users.CountAsync(u => u.LockoutEnd > DateTimeOffset.UtcNow);

            return Json(new
            {
                userComplaintsToday = complaintsFromUsers,
                newUsers = await _userManager.Users.CountAsync(u => u.CreatedAt >= today),
                emailsToday = emailsCount,
                totalUsers = totalUsers,
                blockedTotal = await _userManager.Users.CountAsync(u => u.LockoutEnd > DateTimeOffset.UtcNow),
                lastAction = _lastActionTime
            });
        }
        catch
        {
            return Json(new { emailsToday = 0, totalUsers = 0, lastAction = "Error" });
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetEmailSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2) return Json(new List<object>());
        var users = await _userService.GetAllUsersAsync(null, query);
        return Json(users.Select(u => new { u.Email, u.UserName }).Take(5));
    }
    // 1. Видалення всього сету
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSetByAdmin(int setId)
    {
        var set = await _unitOfWork.Sets.GetByIdAsync(setId);
        if (set != null)
        {
            _unitOfWork.Sets.Delete(set);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Сет повністю видалено";
        }
        return RedirectToAction("Reports");
    }

    // 2. Видалення однієї картки
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCardByAdmin(int cardId, int setId)
    {
        var card = await _unitOfWork.Flashcards.GetByIdAsync(cardId);
        if (card != null)
        {
            _unitOfWork.Flashcards.Delete(card);
            await _unitOfWork.SaveChangesAsync();
        }
        return RedirectToAction("SetDetails", new { id = setId });
    }

    // 3. Редагування картки (Нове)
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
            TempData["Success"] = "Картку відредаговано";
        }
        return RedirectToAction("SetDetails", new { id = setId });
    }
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
            var category = new Category { Name = name.Trim() };
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Категорію створено";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category != null)
        {
            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Категорію видалено";
        }
        return RedirectToAction(nameof(Categories));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(int id, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category != null)
            {
                category.Name = name.Trim();
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Категорію успішно оновлено";
            }
        }
        return RedirectToAction(nameof(Categories));
    }
    public IActionResult Settings() => View();
}