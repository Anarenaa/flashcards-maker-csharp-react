using Core.Context;
using Core.Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(DataContext context) : base(context) { }
    }
}
