using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Sets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BanEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "834371C8-1F0A-44C1-903D-94D1898E5E7B", "User", "USER" },
                    { 2, "0790DE8E-983C-435E-9804-6334D976451B", "Admin", "ADMIN" }
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BanEndDate", "BanReason", "IsBanned" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BanEndDate", "BanReason", "IsBanned" },
                values: new object[] { null, null, false });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AvatarUrl", "BanEndDate", "BanReason", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "IsBanned", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[] { 13, 0, null, null, null, "0790DE8E-983C-435E-9804-6334D976451B", new DateTime(2023, 1, 2, 13, 30, 0, 0, DateTimeKind.Local), "admin@example.com", true, false, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5", null, false, "834371C8-1F0A-44C1-903D-94D1898E5E7B", false, new DateTime(2023, 1, 2, 13, 30, 0, 0, DateTimeKind.Local), "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 13 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 13 });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DropColumn(
                name: "BanEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BanReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Sets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
