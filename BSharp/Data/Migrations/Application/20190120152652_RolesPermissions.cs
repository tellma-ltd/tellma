using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class RolesPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Code = table.Column<string>(maxLength: 255, nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 450, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Views",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Views", x => x.Id);
                    table.UniqueConstraint("AK_Views_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false),
                    ViewId = table.Column<string>(maxLength: 255, nullable: false),
                    Level = table.Column<string>(maxLength: 255, nullable: false),
                    Criteria = table.Column<string>(maxLength: 1024, nullable: true),
                    Memo = table.Column<string>(maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 450, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_TenantId_RoleId",
                        columns: x => new { x.TenantId, x.RoleId },
                        principalTable: "Roles",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_RoleId",
                table: "Permissions",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Code",
                table: "Roles",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name2",
                table: "Roles",
                columns: new[] { "TenantId", "Name2" },
                unique: true,
                filter: "[Name2] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Views");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
