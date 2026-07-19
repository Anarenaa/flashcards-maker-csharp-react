using Core.Extensions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Context
{
    public abstract class BaseDataContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<FlashcardContext> FlashcardContexts { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CardProgress> CardProgresses { get; set; }

        protected BaseDataContext(DbContextOptions options)
           : base(options)
        {
        }
        protected void ConfigureDateTypeForBaseModels(ModelBuilder modelBuilder, string dateSql)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseModel).IsAssignableFrom(entity.ClrType))
                {
                    modelBuilder.Entity(entity.ClrType).Property(nameof(BaseModel.CreatedAt)).HasDefaultValueSql(dateSql);
                    modelBuilder.Entity(entity.ClrType).Property(nameof(BaseModel.UpdatedAt)).HasDefaultValueSql(dateSql);
                }
            }

            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql(dateSql);
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql(dateSql);

            modelBuilder.Entity<CardProgress>().Property(cp => cp.LastReview).HasDefaultValueSql(dateSql);
            modelBuilder.Entity<CardProgress>().Property(cp => cp.NextReview).HasDefaultValueSql(dateSql);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CardProgress>()
                .HasKey(cp => new { cp.UserId, cp.FlashcardId });

            
            modelBuilder.Entity<Set>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sets)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany() 
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CardProgress>()
                .HasOne(cp => cp.Flashcard)
                .WithMany(f => f.CardProgresses)
                .HasForeignKey(cp => cp.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);

            ModelBuilderSeedExtension.SeedAll(modelBuilder);
        }
    }
}
