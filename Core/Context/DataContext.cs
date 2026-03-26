using Core.Models;
using Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Core.Context
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
           : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Шукаємо всі сутності, які наслідують BaseModel, і налаштовуємо їхні поля
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseModel).IsAssignableFrom(entity.ClrType))
                {
                    modelBuilder.Entity(entity.ClrType)
                        .Property(nameof(BaseModel.CreatedAt))
                        .HasDefaultValueSql("GETUTCDATE()");
                    modelBuilder.Entity(entity.ClrType)
                        .Property(nameof(BaseModel.UpdatedAt))
                        .HasDefaultValueSql("GETUTCDATE()");
                }
            }

            ModelBuilderSeedExtension.SeedAll(modelBuilder);
        }
    }
}
