using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Core.Context
{
    public class PostgreSqlContextFactory : IDesignTimeDbContextFactory<PostgreSqlDataContext>
    {
        public PostgreSqlDataContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "/../WebApi")
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSqlConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDataContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new PostgreSqlDataContext(optionsBuilder.Options);
        }
    }
}