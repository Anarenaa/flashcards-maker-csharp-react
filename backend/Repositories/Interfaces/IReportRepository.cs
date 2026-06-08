using Core.Models;

namespace Repositories.Interfaces
{
    public interface IReportRepository : IRepository<Report>
    {
        Task DeleteUserReports(int userId);
        Task<Dictionary<int, int>> GetReportsCountPerUserAsync();
    }
}
