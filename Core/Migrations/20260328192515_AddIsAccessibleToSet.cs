using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAccessibleToSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccessible",
                table: "Sets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsAccessible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsAccessible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsAccessible",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccessible",
                table: "Sets");
        }
    }
}
