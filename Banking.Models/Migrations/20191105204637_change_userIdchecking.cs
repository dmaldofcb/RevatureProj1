using Microsoft.EntityFrameworkCore.Migrations;

namespace RevatureProj1.Data.Migrations
{
    public partial class change_userIdchecking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "UserID",
                table: "CheckingAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "CheckingAccounts");

            migrationBuilder.AddColumn<string>(
                name: "UserBankingId",
                table: "CheckingAccounts",
                type: "nvarchar(450)",
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
    }
}
