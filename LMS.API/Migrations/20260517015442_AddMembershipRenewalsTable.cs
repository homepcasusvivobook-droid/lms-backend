using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.API.Migrations
{
    public partial class AddMembershipRenewalsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembershipRenewals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    MemberDbId = table.Column<int>(type: "int", nullable: false),

                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),

                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),

                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),

                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),

                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),

                    IsActive = table.Column<bool>(type: "bit", nullable: false),

                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),

                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),

                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),

                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),

                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipRenewals", x => x.Id);

                    table.ForeignKey(
                        name: "FK_MembershipRenewals_Members_MemberDbId",
                        column: x => x.MemberDbId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRenewals_MemberDbId",
                table: "MembershipRenewals",
                column: "MemberDbId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipRenewals");
        }
    }
}