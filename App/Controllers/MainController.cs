using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;
using System;
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
        private readonly IUnitOfWork _unitOfWork;

        public MainController(SetService setService, CollectionService collectionService, IUnitOfWork unitOfWork)
        {
            _setService = setService;
            _collectionService = collectionService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? searchText, string sortOrder = "newest")
        {
            if (User.IsInRole("Admin"))
            {

                return RedirectToAction("Users", "Admin");
            }

            var sets = await _setService.GetAllSetsAsync(UserId, null, searchText);

            sets = sortOrder switch
            {
                "oldest" => sets.OrderBy(s => s.CreatedAt).ToList(),
                "newest" => sets.OrderByDescending(s => s.CreatedAt).ToList(),
                _ => sets.OrderByDescending(s => s.CreatedAt).ToList(),
            };

            ViewData["CurrentFilter"] = searchText;
            ViewData["CurrentSort"] = sortOrder;

            if (UserId > 0)
            {
                ViewBag.UserCollections = await _collectionService.GetCollectionsByUserIdAsync(UserId);
            }

            return View(sets);
        }

        // Додавання сету в колекцію
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

        // Створення скарги
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