using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RevatureProj1.Data.Migrations
{
    public partial class trans_time : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeTransaction",
                table: "AccountTransactions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeTransaction",
                table: "AccountTransactions");
        }
    }
}
