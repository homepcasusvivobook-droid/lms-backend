using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBookPurchaseDetailIdToBookCopies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookPurchaseDetailId",
                table: "BookCopies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_BookPurchaseDetailId",
                table: "BookCopies",
                column: "BookPurchaseDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_BookPurchaseDetails_BookPurchaseDetailId",
                table: "BookCopies",
                column: "BookPurchaseDetailId",
                principalTable: "BookPurchaseDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_BookPurchaseDetails_BookPurchaseDetailId",
                table: "BookCopies");

            migrationBuilder.DropIndex(
                name: "IX_BookCopies_BookPurchaseDetailId",
                table: "BookCopies");

            migrationBuilder.DropColumn(
                name: "BookPurchaseDetailId",
                table: "BookCopies");
        }
    }
}
