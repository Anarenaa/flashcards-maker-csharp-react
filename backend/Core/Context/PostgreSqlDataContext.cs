using Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Core.Context
{
    public class PostgreSqlDataContext : BaseDataContext
    {
        public PostgreSqlDataContext(DbContextOptions<PostgreSqlDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureDateTypeForBaseModels(modelBuilder, "NOW()");
        }
    }
}
