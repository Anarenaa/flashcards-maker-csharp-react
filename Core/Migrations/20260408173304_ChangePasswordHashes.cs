using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class ChangePasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECWiT3PBdFR8jFPQAbbr/xZW0nPeypMwcicb5XAnXqxtp7h3mVfSfN4t2UQEzlwEcg==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEzkdq2JyhFWGCHP/KtVdIRcOheqDIHWulCLEYW1h6RefsLHVko0jduu4Zcu2Bwt1g==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 13,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFgsHMTnZXkcaw1E4PQs9bvYxtiRXQo18Lr3rmtablo5pOsn5PC6XEs7CiZIZKTkTA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 13,
                column: "PasswordHash",
                value: "AQAAAAEAACcQAAAAEKqYkx8X+OZkG8B3JzQX5Y3Z7Q9W8V5N2M1K4P6R0T3U7I9S2L5");
        }
    }
}
