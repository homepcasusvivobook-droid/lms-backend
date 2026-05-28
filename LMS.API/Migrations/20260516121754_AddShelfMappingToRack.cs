using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfMappingToRack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShelfId",
                table: "Racks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Racks_ShelfId",
                table: "Racks",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_Racks_Shelves_ShelfId",
                table: "Racks",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Racks_Shelves_ShelfId",
                table: "Racks");

            migrationBuilder.DropIndex(
                name: "IX_Racks_ShelfId",
                table: "Racks");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "Racks");
        }
    }
}
