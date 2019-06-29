using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class LocalUserSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalUserSettings",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    Key = table.Column<string>(maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))"),
                    Value = table.Column<string>(maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalUserSettings", x => new { x.TenantId, x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_LocalUserSettings_LocalUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalUserSettings");
        }
    }
}
