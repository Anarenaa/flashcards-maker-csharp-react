using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;
using Services.Interfaces; // Додано для IPracticeService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Authorize]
    public class MainController : BaseController
    {
        private readonly SetService _setService;
        private readonly CollectionService _collectionService;
        private readonly CategoryService _categoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IPracticeService _practiceService; 

        public MainController(
            SetService setService,
            CollectionService collectionService,
            CategoryService categoryService,
            IUnitOfWork unitOfWork,
            UserService userService,
            UserManager<User> userManager,
            IPracticeService practiceService) 
        {
            _setService = setService;
            _collectionService = collectionService;
            _categoryService = categoryService;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _userManager = userManager;
            _practiceService = practiceService;
        }

        public async Task<IActionResult> Index(string? searchText, int? categoryId, string sortOrder = "newest", string filter = "all")
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Users", "Admin");
            }

            List<int>? categoryIds = categoryId.HasValue ? new List<int> { categoryId.Value } : null;

            // 1. Отримуємо всі доступні сети
            var sets = await _setService.GetAllSetsAsync(UserId, categoryIds, searchText);

            // 3. Фільтрація по табах (працює тільки коли тицяєш на конкретний таб)
            sets = filter switch
            {
                "notstarted" => sets.Where(s => s.Progress == 0).ToList(),
                "inprogress" => sets.Where(s => s.Progress > 0 && s.Progress < 100).ToList(),
                "finished" => sets.Where(s => s.Progress == 100).ToList(),
                _ => sets.ToList() // "all" показує абсолютно все
            };

            // 4. Сортування
            sets = sortOrder switch
            {
                "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
                _ => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            };

            // 5. Аватари
            var authorNames = sets.Select(s => s.UserName).Distinct();
            var avatarMap = new Dictionary<string, string>();
            foreach (var name in authorNames)
            {
                var user = await _userManager.FindByNameAsync(name);
                if (user != null)
                {
                    var profile = await _userService.GetUserProfileAsync(user.Id);
                    avatarMap[name] = profile.AvatarUrl;
                }
            }
            ViewBag.AuthorAvatars = avatarMap;

            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CurrentFilter"] = searchText;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["ActiveTab"] = filter;

            if (UserId > 0)
            {
                ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(UserId);
            }

            return View(sets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCollection(int setId, int? collectionId, string? newCollectionName)
        {
            if (User.IsInRole("Admin")) return Forbid();

            int finalCollectionId = collectionId ?? 0;

            if (!string.IsNullOrWhiteSpace(newCollectionName))
            {
                var newCol = new CollectionDTO { Name = newCollectionName.Trim() };
                await _collectionService.CreateCollectionAsync(newCol, UserId);

                var userCollections = await _collectionService.GetCollectionsByUserIdAsync(UserId);
                var createdCol = userCollections.FirstOrDefault(c => c.Name == newCollectionName.Trim());

                if (createdCol != null) finalCollectionId = createdCol.Id.Value;
            }

            if (setId != 0 && finalCollectionId != 0)
            {
                await _setService.AddSetToCollectionAsync(setId, finalCollectionId);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReport(CreateReportDTO dto)
        {
            if (dto.ReportedUserId == null && dto.ReportedSetId == null)
            {
                TempData["ErrorMessage"] = "Об'єкт скарги не вказано";
                return RedirectToAction("Index");
            }

            try
            {
                var report = new Report
                {
                    ReporterId = UserId,
                    ReportedUserId = dto.ReportedUserId,
                    ReportedSetId = dto.ReportedSetId,
                    Reason = dto.Reason,
                    CustomReason = dto.CustomReason,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Вашу скаргу надіслано на розгляд модераторам.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при відправці скарги: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}