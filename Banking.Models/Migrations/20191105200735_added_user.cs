using Microsoft.EntityFrameworkCore.Migrations;

namespace RevatureProj1.Data.Migrations
{
    public partial class added_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckingAccounts_AspNetUsers_UserId",
                table: "CheckingAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CheckingAccounts_UserId",
                table: "CheckingAccounts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CheckingAccounts");

            migrationBuilder.AddColumn<string>(
                name: "UserBankingId",
                table: "CheckingAccounts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckingAccounts_UserBankingId",
                table: "CheckingAccounts",
                column: "UserBankingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckingAccounts_AspNetUsers_UserBankingId",
                table: "CheckingAccounts",
                column: "UserBankingId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckingAccounts_AspNetUsers_UserBankingId",
                table: "CheckingAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CheckingAccounts_UserBankingId",
                table: "CheckingAccounts");

            migrationBuilder.DropColumn(
                name: "UserBankingId",
                table: "CheckingAccounts");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CheckingAccounts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckingAccounts_UserId",
                table: "CheckingAccounts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckingAccounts_AspNetUsers_UserId",
                table: "CheckingAccounts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
