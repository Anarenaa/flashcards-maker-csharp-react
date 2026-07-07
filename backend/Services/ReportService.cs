using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories;
using Repositories.Interfaces;

namespace Services
{
    public class ReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public ReportService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        //Замінити Report на ReportDTO
        public async Task<IEnumerable<Report>> GetAllReports()
        {
            var reports = await _unitOfWork.Reports.GetAllAsync(filter: r => r.IsResolved == false);
            return reports;
        }
        public async Task<Report?> GetReportByIdAsync(int reportId)
        {
            return await _unitOfWork.Reports.GetByIdAsync(reportId);
        }
        public async Task CreateReportAsync(CreateReportDTO report, int currentUserId)
        {
            if (!report.ReportedUserId.HasValue && !report.ReportedSetId.HasValue)
                throw new ArgumentException("Потрібно вказати користувача або сет");
            if (report.ReportedUserId == currentUserId)
                throw new ArgumentException("Не можна скаржитися на себе");
            if (report.Reason == ReportReason.Other && string.IsNullOrEmpty(report.CustomReason))
                throw new ArgumentException("Для причини 'Інше' потрібен опис");

            var newReport = new Report
            {
                ReporterId = currentUserId,
                ReportedUserId = report.ReportedUserId,
                ReportedSetId = report.ReportedSetId,
                Reason = report.Reason,
                CustomReason = report.CustomReason
            };
            await _unitOfWork.Reports.AddAsync(newReport);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task ResolveReportAsync(int reportId)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
            if (report != null)
            {
                report.IsResolved = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }
        public async Task DeleteReportAsync(int reportId)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
            if (report == null) throw new ArgumentException("Скарга не знайдена");
            _unitOfWork.Reports.Delete(report);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<Dictionary<int, int>> GetReportsCountPerUserAsync()
        {
            return await _unitOfWork.Reports.GetReportsCountPerUserAsync();
        }
        public async Task<List<Report>> GetAdminWarningsAsync()
        {
            var reports = await _unitOfWork.Reports.GetAllAsync(includeProperties: "Reporter,ReportedUser,ReportedSet");
            var warnings = new List<Report>();

            foreach (var r in reports)
            {
                if (r.Reporter != null && await _userManager.IsInRoleAsync(r.Reporter, "Admin"))
                    warnings.Add(r);
            }

            return warnings.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<List<Report>> GetUserComplaintsAsync()
        {
            var reports = await _unitOfWork.Reports.GetAllAsync(includeProperties: "Reporter,ReportedUser,ReportedSet");
            var complaints = new List<Report>();

            foreach (var r in reports)
            {
                if (r.Reporter != null && !await _userManager.IsInRoleAsync(r.Reporter, "Admin"))
                    complaints.Add(r);
            }

            return complaints.OrderByDescending(x => x.CreatedAt).ToList();
        }
    }
}
