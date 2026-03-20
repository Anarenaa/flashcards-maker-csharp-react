using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Extensions
{
    public class ModelBuilderSeedExtension
    {
        public static void SeedAll(ModelBuilder modelBuilder)
        {
            SeedSets(modelBuilder);
            SeedFlashcards(modelBuilder);
            SeedCategories(modelBuilder);
            SeedCollections(modelBuilder);

            SeedCategorySets(modelBuilder);
            SeedCollectionSets(modelBuilder);
        }
        public static void SeedSets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Set>().HasData(
                new Set
                {
                    Id = 1,
                    Name = "Fruits",
                    Description = "English vocabulary",
                    CreatedAt = DateTime.Parse("2024-01-01T10:15:00"),
                    UpdatedAt = DateTime.Parse("2024-01-01T10:15:00")
                },
                new Set
                {
                    Id = 2,
                    Name = "Math",
                    Description = "Math test preparation set",
                    IsPublic = false,
                    CreatedAt = DateTime.Parse("2024-01-02T11:30:00"),
                    UpdatedAt = DateTime.Parse("2024-01-03T12:11:00")
                },
                new Set
                {
                    Id = 3,
                    Name = "Роки революцій",
                    Description = "Всесвітня історія: революції різних років",
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
                    Name = "Favorites"
                },
                new Collection
                {
                    Id = 2,
                    Name = "My English Vocabulary"
                },
                new Collection
                {
                    Id = 3,
                    Name = "English A0"
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
