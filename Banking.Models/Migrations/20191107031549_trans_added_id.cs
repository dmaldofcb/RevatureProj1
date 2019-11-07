using Microsoft.EntityFrameworkCore.Migrations;

namespace RevatureProj1.Data.Migrations
{
    public partial class trans_added_id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "AccountTransactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "AccountTransactions");
        }
    }
}
