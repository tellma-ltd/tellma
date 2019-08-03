using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class ProductCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))"),
                    ParentId = table.Column<int>(nullable: true),
                    Node = table.Column<string>(nullable: true),
                    Level = table.Column<short>(nullable: false),
                    ParentNode = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Name3 = table.Column<string>(maxLength: 255, nullable: true),
                    Code = table.Column<string>(maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedById = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))"),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedById = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProductCategories_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCategories_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_TenantId_ParentId",
                        columns: x => new { x.TenantId, x.ParentId },
                        principalTable: "ProductCategories",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_TenantId_Code",
                table: "ProductCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_TenantId_CreatedById",
                table: "ProductCategories",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_TenantId_ModifiedById",
                table: "ProductCategories",
                columns: new[] { "TenantId", "ModifiedById" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_TenantId_ParentId",
                table: "ProductCategories",
                columns: new[] { "TenantId", "ParentId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategories");
        }
    }
}
