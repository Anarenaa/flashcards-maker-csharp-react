using Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Core.Context
{
    public class SqlServerDataContext : BaseDataContext
    {
        public SqlServerDataContext(DbContextOptions<SqlServerDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureDateTypeForBaseModels(modelBuilder, "GETUTCDATE()");
        }
    }
}
