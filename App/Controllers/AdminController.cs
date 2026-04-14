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
    private static int _sentEmailsCount = 0;
    private static string _lastActionTime = "Ще не було";

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

    // --- 1. КЕРУВАННЯ КОРИСТУВАЧАМИ ---
    public async Task<IActionResult> Users(string searchTerm)
    {
        var allUsers = await _userService.GetAllUsersAsync(null, searchTerm);

        var reports = await _unitOfWork.Reports.GetAllAsync();
        var warningCounts = reports
            .Where(r => r.ReportedUserId.HasValue)
            .GroupBy(r => r.ReportedUserId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var blockedUserIds = await _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .Select(u => u.Id)
            .ToListAsync();

        ViewBag.WarningCounts = warningCounts;
        ViewBag.BlockedUserIds = blockedUserIds;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("UserTableRows", allUsers);
        }

        return View(allUsers);
    }

    public async Task<IActionResult> UserDetails(int id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = "Користувача видалено назавжди";
        return RedirectToAction("Users");
    }

    // --- 2. БЛОКУВАННЯ ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, bool shouldBlock)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, shouldBlock ? DateTimeOffset.MaxValue : null);
            _lastActionTime = DateTime.Now.ToString("HH:mm");
        }
        return RedirectToAction(shouldBlock ? "Users" : "Blocked");
    }

    // --- 3. ЦЕНТР МОДЕРАЦІЇ (СКАРГИ) ---
    public async Task<IActionResult> Reports(int? userId)
    {
        if (userId.HasValue)
        {
            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user != null) ViewBag.TargetEmail = user.Email;
        }

        var allReports = await _unitOfWork.Reports.GetAllAsync(
            includeProperties: "Reporter,ReportedUser,ReportedSet"
        );

        var adminWarnings = new List<Report>();
        var userComplaints = new List<Report>();

        foreach (var r in allReports)
        {
            var isReporterAdmin = await _userManager.IsInRoleAsync(r.Reporter, "Admin");
            if (isReporterAdmin) adminWarnings.Add(r);
            else userComplaints.Add(r);
        }

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
            var systemReport = new Report
            {
                ReporterId = adminId,
                ReportedUserId = targetUser.Id,
                Reason = ReportReason.Other,
                CustomReason = "Системне попередження: " + subject,
                CreatedAt = DateTime.UtcNow,
                IsResolved = true
            };
            await _unitOfWork.Reports.AddAsync(systemReport);
            await _unitOfWork.SaveChangesAsync();
        }

        _sentEmailsCount++;
        _lastActionTime = DateTime.Now.ToString("HH:mm");
        TempData["Success"] = $"Лист надіслано для {email}";
        return RedirectToAction("Reports");
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
        return RedirectToAction("Reports");
    }

    // --- 4. ДЕТАЛІ СЕТУ ---
    public async Task<IActionResult> SetDetails(int id)
    {
        var setDetails = await _setService.GetSetByIdAsync(id);
        if (setDetails == null) return NotFound();
        return View(setDetails);
    }

    // --- 5. ІНШЕ (API & Налаштування) ---
    public IActionResult Settings() => View();

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
}