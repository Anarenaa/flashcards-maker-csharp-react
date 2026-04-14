using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(DataContext context) : base(context) { }

        public async Task DeleteUserReports(int userId)
        {
            await _dbSet.Where(r =>
                r.ReporterId == userId ||
                r.ReportedUserId == userId ||
                (r.ReportedSetId != null && r.ReportedSet.UserId == userId))
                .ExecuteDeleteAsync();
        }
        public async Task<Dictionary<int, int>> GetReportsCountPerUserAsync()
        {
            return await _dbSet
                .Where(r => r.ReportedUserId.HasValue)
                .GroupBy(r => r.ReportedUserId!.Value)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);
        }
    }
}
