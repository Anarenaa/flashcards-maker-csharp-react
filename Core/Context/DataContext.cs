using Core.Models;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Your server");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
