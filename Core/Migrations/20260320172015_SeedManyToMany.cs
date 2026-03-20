using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CategorySet",
                columns: new[] { "CategoriesId", "SetsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "CollectionSet",
                columns: new[] { "CollectionsId", "SetsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 1 },
                    { 3, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CategorySet",
                keyColumns: new[] { "CategoriesId", "SetsId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "CategorySet",
                keyColumns: new[] { "CategoriesId", "SetsId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "CategorySet",
                keyColumns: new[] { "CategoriesId", "SetsId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "CollectionSet",
                keyColumns: new[] { "CollectionsId", "SetsId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "CollectionSet",
                keyColumns: new[] { "CollectionsId", "SetsId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "CollectionSet",
                keyColumns: new[] { "CollectionsId", "SetsId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "CollectionSet",
                keyColumns: new[] { "CollectionsId", "SetsId" },
                keyValues: new object[] { 3, 1 });
        }
    }
}
