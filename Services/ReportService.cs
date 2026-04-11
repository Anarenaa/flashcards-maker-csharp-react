using Core.DTOs;
using Core.Models;
using Repositories.Interfaces;

namespace Services
{
    public class ReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Report>> GetAllReports()
        {
            return await _unitOfWork.Reports.GetAllAsync(r => r.IsResolved == false);
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
            if (report != null)
            {
                _unitOfWork.Reports.Delete(report);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
