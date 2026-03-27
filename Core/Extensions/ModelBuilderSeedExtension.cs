using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Extensions
{
    public class ModelBuilderSeedExtension
    {
        public static void SeedAll(ModelBuilder modelBuilder)
        {
            SeedUsers(modelBuilder);
            SeedSets(modelBuilder);
            SeedFlashcards(modelBuilder);
            SeedCategories(modelBuilder);
            SeedCollections(modelBuilder);

            SeedCategorySets(modelBuilder);
            SeedCollectionSets(modelBuilder);
        }
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            var user1 = new User
            {
                Id = 1,
                UserName = "john_doe",
                NormalizedUserName = "JOHN_DOE",
                Email = "john_doe@gmail.com",
                NormalizedEmail = "JOHN_DOE@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = "834371C8-1F0A-44C1-903D-94D1898E5E7B",
                ConcurrencyStamp = "0790DE8E-983C-435E-9804-6334D976451B",
                CreatedAt = DateTime.Parse("2023-01-02T11:30:00Z"),
                UpdatedAt = DateTime.Parse("2023-01-02T11:30:00Z"),
                PasswordHash = "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5"
            };

            var user2 = new User
            {
                Id = 2,
                UserName = "jane_smith",
                NormalizedUserName = "JANE_SMITH",
                Email = "jane_smith@gmail.com",
                NormalizedEmail = "JANE_SMITH@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = "0B4D1A60-F22B-4467-93C0-94D1898E5E7C",
                ConcurrencyStamp = "C8A1088E-983C-435E-9804-6334D976451C",
                CreatedAt = DateTime.Parse("2023-02-02T11:30:00Z"),
                UpdatedAt = DateTime.Parse("2023-03-02T11:30:00Z"),
                PasswordHash = "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5"
            };

            modelBuilder.Entity<User>().HasData(user1, user2);
        }
        public static void SeedSets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Set>().HasData(
                new Set
                {
                    Id = 1,
                    Name = "Fruits",
                    Description = "English vocabulary",
                    UserId = 1,
                    CreatedAt = DateTime.Parse("2024-01-01T10:15:00"),
                    UpdatedAt = DateTime.Parse("2024-01-01T10:15:00")
                },
                new Set
                {
                    Id = 2,
                    Name = "Math",
                    Description = "Math test preparation set",
                    IsPublic = false,
                    UserId = 2,
                    CreatedAt = DateTime.Parse("2024-01-02T11:30:00"),
                    UpdatedAt = DateTime.Parse("2024-01-03T12:11:00")
                },
                new Set
                {
                    Id = 3,
                    Name = "Роки революцій",
                    Description = "Всесвітня історія: революції різних років",
                    UserId = 2,
                    CreatedAt = DateTime.Parse("2024-01-03T09:45:00"),
                    UpdatedAt = DateTime.Parse("2024-01-04T14:20:00")
                }
            );
        }
        public static void SeedFlashcards(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flashcard>().HasData(
                new Flashcard
                {
                    Id = 1,
                    Term = "Apple",
                    Definition = "Яблуко",
                    SetId = 1,
                    CreatedAt = DateTime.Parse("2026-01-01T10:20:00"),
                    UpdatedAt = DateTime.Parse("2026-01-01T10:20:00")
                },
                new Flashcard
                {
                    Id = 2,
                    Term = "Banana",
                    Definition = "Банан",
                    SetId = 1,
                    CreatedAt = DateTime.Parse("2026-01-01T10:25:00"),
                    UpdatedAt = DateTime.Parse("2026-01-01T10:25:00")
                },
                new Flashcard
                {
                    Id = 3,
                    Term = "Pineapple",
                    Definition = "Ананас",
                    SetId = 1,
                    CreatedAt = DateTime.Parse("2026-01-01T10:30:00"),
                    UpdatedAt = DateTime.Parse("2026-01-01T10:30:00")
                },
                new Flashcard
                {
                    Id = 4,
                    Term = "Дискримінант",
                    Definition = "Це числова характеристика квадратного рівняння, що визначає кількість його дійсних коренів.",
                    SetId = 2,
                    CreatedAt = DateTime.Parse("2026-01-01T10:35:00"),
                    UpdatedAt = DateTime.Parse("2026-01-02T11:35:00")
                },
                new Flashcard
                {
                    Id = 5,
                    Term = "Інтеграл",
                    Definition = "Це математична операція, що дозволяє знайти площу під кривою або обчислити загальну кількість чогось на основі швидкості зміни.",
                    SetId = 2,
                    CreatedAt = DateTime.Parse("2026-01-02T11:40:00"),
                    UpdatedAt = DateTime.Parse("2026-01-02T11:40:00")
                },
                new Flashcard
                {
                    Id = 6,
                    Term = "Революція",
                    Definition = "Це радикальна зміна в політичній, соціальній або економічній системі, яка зазвичай відбувається швидко і часто супроводжується конфліктами.",
                    SetId = 3,
                    CreatedAt = DateTime.Parse("2024-01-03T09:50:00"),
                    UpdatedAt = DateTime.Parse("2024-01-04T09:50:00")
                },
                new Flashcard
                {
                    Id = 7,
                    Term = "Роки великої французької революції",
                    Definition = "1789-1799",
                    SetId = 3,
                    CreatedAt = DateTime.Parse("2026-01-03T09:55:00"),
                    UpdatedAt = DateTime.Parse("2026-01-06T09:55:00")
                },
                new Flashcard
                {
                    Id = 8,
                    Term = "З якого по який рік тривала \"Весна народів\" у Європі?",
                    Definition = "1848-1849",
                    SetId = 3,
                    CreatedAt = DateTime.Parse("2026-01-03T10:00:00"),
                    UpdatedAt = DateTime.Parse("2026-01-03T10:00:00")
                }
            );
        }
        public static void SeedCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "English"
                },
                new Category
                {
                    Id = 2,
                    Name = "Vocabulary"
                },
                new Category
                {
                    Id = 3,
                    Name = "Math"
                }
            );
        }
        public static void SeedCollections(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Collection>().HasData(
                new Collection
                {
                    Id = 1,
                    Name = "Favorites",
                    UserId = 2
                },
                new Collection
                {
                    Id = 2,
                    Name = "My English Vocabulary",
                    UserId = 1
                },
                new Collection
                {
                    Id = 3,
                    Name = "English A0",
                    UserId = 1
                }
            );
        }

        public static void SeedCategorySets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity("CategorySet").HasData(
                new { CategoriesId = 1, SetsId = 1 },
                new { CategoriesId = 2, SetsId = 1 },
                new { CategoriesId = 3, SetsId = 2 }
            );
        }
        public static void SeedCollectionSets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity("CollectionSet").HasData(
                new { CollectionsId = 1, SetsId = 1 },
                new { CollectionsId = 1, SetsId = 2 },
                new { CollectionsId = 2, SetsId = 1 },
                new { CollectionsId = 3, SetsId = 1 }
            );
        }
    }
}
