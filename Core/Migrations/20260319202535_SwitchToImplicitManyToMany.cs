using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToImplicitManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sets_Collections_CollectionId",
                table: "Sets");

            migrationBuilder.DropTable(
                name: "CollectionSets");

            migrationBuilder.DropTable(
                name: "SetCategories");

            migrationBuilder.DropIndex(
                name: "IX_Sets_CollectionId",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Sets");

            migrationBuilder.CreateTable(
                name: "CollectionSet",
                columns: table => new
                {
                    CollectionsId = table.Column<int>(type: "int", nullable: false),
                    SetsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSet", x => new { x.CollectionsId, x.SetsId });
                    table.ForeignKey(
                        name: "FK_CollectionSet_Collections_CollectionsId",
                        column: x => x.CollectionsId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionSet_Sets_SetsId",
                        column: x => x.SetsId,
                        principalTable: "Sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSet_SetsId",
                table: "CollectionSet",
                column: "SetsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionSet");

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Sets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollectionSets",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    SetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSets", x => new { x.CollectionId, x.SetId });
                    table.ForeignKey(
                        name: "FK_CollectionSets_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionSets_Sets_SetId",
                        column: x => x.SetId,
                        principalTable: "Sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetCategories",
                columns: table => new
                {
                    SetId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetCategories", x => new { x.SetId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_SetCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetCategories_Sets_SetId",
                        column: x => x.SetId,
                        principalTable: "Sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sets_CollectionId",
                table: "Sets",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSets_SetId",
                table: "CollectionSets",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_SetCategories_CategoryId",
                table: "SetCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sets_Collections_CollectionId",
                table: "Sets",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id");
        }
    }
}
