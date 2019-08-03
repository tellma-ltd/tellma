using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class AddMask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mask",
                table: "Permissions",
                maxLength: 2048,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mask",
                table: "Permissions");
        }
    }
}
