using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class AddReportedSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReportedUserId",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ReportedSetId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedSetId",
                table: "Reports",
                column: "ReportedSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Sets_ReportedSetId",
                table: "Reports",
                column: "ReportedSetId",
                principalTable: "Sets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Sets_ReportedSetId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportedSetId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportedSetId",
                table: "Reports");

            migrationBuilder.AlterColumn<int>(
                name: "ReportedUserId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
