using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSetsTableAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "set_id",
                table: "flashcards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "sets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "varchar(500)", nullable: true),
                    type = table.Column<string>(type: "varchar(20)", nullable: false),
                    from_lang = table.Column<string>(type: "varchar(2)", nullable: false),
                    to_lang = table.Column<string>(type: "varchar(2)", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    is_generated = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flashcards_set_id",
                table: "flashcards",
                column: "set_id");

            migrationBuilder.AddForeignKey(
                name: "fk_flashcards_sets_set_id",
                table: "flashcards",
                column: "set_id",
                principalTable: "sets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_flashcards_sets_set_id",
                table: "flashcards");

            migrationBuilder.DropTable(
                name: "sets");

            migrationBuilder.DropIndex(
                name: "ix_flashcards_set_id",
                table: "flashcards");

            migrationBuilder.DropColumn(
                name: "set_id",
                table: "flashcards");
        }
    }
}
