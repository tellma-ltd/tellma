using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class ProfilePics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageId",
                table: "LocalUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => new { x.Id, x.TenantId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_Id",
                table: "Blobs",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "LocalUsers");
        }
    }
}
