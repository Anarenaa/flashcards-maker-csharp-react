using Core.Extensions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Context
{
    public class DataContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
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
            
            modelBuilder.Entity<Set>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sets)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

                
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            ModelBuilderSeedExtension.SeedAll(modelBuilder);
        }
    }
}
