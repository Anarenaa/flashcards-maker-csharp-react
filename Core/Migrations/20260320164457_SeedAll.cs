using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, "English", null },
                    { 2, "Vocabulary", null },
                    { 3, "Math", null }
                });

            migrationBuilder.InsertData(
                table: "Collections",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(6988), null, "Favorites", new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(6990), null },
                    { 2, new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(7668), null, "My English Vocabulary", new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(7670), null },
                    { 3, new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(7673), null, "English A0", new DateTime(2026, 3, 20, 16, 44, 55, 908, DateTimeKind.Utc).AddTicks(7674), null }
                });

            migrationBuilder.InsertData(
                table: "Sets",
                columns: new[] { "Id", "CreatedAt", "Description", "IsPublic", "Name", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 10, 15, 0, 0, DateTimeKind.Unspecified), "English vocabulary", true, "Fruits", new DateTime(2024, 1, 1, 10, 15, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, new DateTime(2024, 1, 2, 11, 30, 0, 0, DateTimeKind.Unspecified), "Math test preparation set", false, "Math", new DateTime(2024, 1, 3, 12, 11, 0, 0, DateTimeKind.Unspecified), null },
                    { 3, new DateTime(2024, 1, 3, 9, 45, 0, 0, DateTimeKind.Unspecified), "Всесвітня історія: революції різних років", true, "Роки революцій", new DateTime(2024, 1, 4, 14, 20, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.InsertData(
                table: "Flashcards",
                columns: new[] { "Id", "CreatedAt", "Definition", "SetId", "Term", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 10, 20, 0, 0, DateTimeKind.Unspecified), "Яблуко", 1, "Apple", new DateTime(2026, 1, 1, 10, 20, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 1, 1, 10, 25, 0, 0, DateTimeKind.Unspecified), "Банан", 1, "Banana", new DateTime(2026, 1, 1, 10, 25, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 1, 1, 10, 30, 0, 0, DateTimeKind.Unspecified), "Ананас", 1, "Pineapple", new DateTime(2026, 1, 1, 10, 30, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2026, 1, 1, 10, 35, 0, 0, DateTimeKind.Unspecified), "Це числова характеристика квадратного рівняння, що визначає кількість його дійсних коренів.", 2, "Дискримінант", new DateTime(2026, 1, 2, 11, 35, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2026, 1, 2, 11, 40, 0, 0, DateTimeKind.Unspecified), "Це математична операція, що дозволяє знайти площу під кривою або обчислити загальну кількість чогось на основі швидкості зміни.", 2, "Інтеграл", new DateTime(2026, 1, 2, 11, 40, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2024, 1, 3, 9, 50, 0, 0, DateTimeKind.Unspecified), "Це радикальна зміна в політичній, соціальній або економічній системі, яка зазвичай відбувається швидко і часто супроводжується конфліктами.", 3, "Революція", new DateTime(2024, 1, 4, 9, 50, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2026, 1, 3, 9, 55, 0, 0, DateTimeKind.Unspecified), "1789-1799", 3, "Роки великої французької революції", new DateTime(2026, 1, 6, 9, 55, 0, 0, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2026, 1, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), "1848-1849", 3, "З якого по який рік тривала \"Весна народів\" у Європі?", new DateTime(2026, 1, 3, 10, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
