using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Views",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Views", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "LocalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    ExternalId = table.Column<string>(maxLength: 450, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Email = table.Column<string>(maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    AgentId = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    CreatedByTenantId = table.Column<int>(nullable: true),
                    CreatedById1 = table.Column<int>(nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false),
                    LastAccess = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalUsers", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_LocalUsers_LocalUsers_CreatedByTenantId_CreatedById1",
                        columns: x => new { x.CreatedByTenantId, x.CreatedById1 },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Custodies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    CustodyType = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Code = table.Column<string>(maxLength: 255, nullable: true),
                    Address = table.Column<string>(maxLength: 1024, nullable: true),
                    BirthDateTime = table.Column<DateTimeOffset>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false),
                    AgentType = table.Column<string>(maxLength: 255, nullable: true),
                    IsRelated = table.Column<bool>(nullable: true, defaultValue: false),
                    TaxIdentificationNumber = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Title2 = table.Column<string>(maxLength: 255, nullable: true),
                    Gender = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Custodies", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Custodies_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Custodies_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementUnits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Code = table.Column<string>(maxLength: 255, nullable: true),
                    UnitType = table.Column<string>(maxLength: 255, nullable: false),
                    UnitAmount = table.Column<double>(nullable: false),
                    BaseAmount = table.Column<double>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnits", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_MeasurementUnits_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeasurementUnits_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

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
                    CreatedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Roles_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roles_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
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
                    CreatedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Permissions_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permissions_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_TenantId_RoleId",
                        columns: x => new { x.TenantId, x.RoleId },
                        principalTable: "Roles",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false),
                    Memo = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMemberships", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_RoleMemberships_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleMemberships_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleMemberships_Roles_TenantId_RoleId",
                        columns: x => new { x.TenantId, x.RoleId },
                        principalTable: "Roles",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMemberships_LocalUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Custodies_TenantId_Code",
                table: "Custodies",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Custodies_TenantId_CreatedById",
                table: "Custodies",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Custodies_TenantId_ModifiedById",
                table: "Custodies",
                columns: new[] { "TenantId", "ModifiedById" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_CreatedByTenantId_CreatedById1",
                table: "LocalUsers",
                columns: new[] { "CreatedByTenantId", "CreatedById1" },
                unique: true,
                filter: "[CreatedByTenantId] IS NOT NULL AND [CreatedById1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_TenantId_AgentId",
                table: "LocalUsers",
                columns: new[] { "TenantId", "AgentId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_TenantId_Email",
                table: "LocalUsers",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_TenantId_ExternalId",
                table: "LocalUsers",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_TenantId_Code",
                table: "MeasurementUnits",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_TenantId_CreatedById",
                table: "MeasurementUnits",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_TenantId_ModifiedById",
                table: "MeasurementUnits",
                columns: new[] { "TenantId", "ModifiedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_CreatedById",
                table: "Permissions",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_ModifiedById",
                table: "Permissions",
                columns: new[] { "TenantId", "ModifiedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_RoleId",
                table: "Permissions",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMemberships_TenantId_CreatedById",
                table: "RoleMemberships",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMemberships_TenantId_ModifiedById",
                table: "RoleMemberships",
                columns: new[] { "TenantId", "ModifiedById" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMemberships_TenantId_RoleId",
                table: "RoleMemberships",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMemberships_TenantId_UserId",
                table: "RoleMemberships",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Code",
                table: "Roles",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_CreatedById",
                table: "Roles",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_IsPublic",
                table: "Roles",
                columns: new[] { "TenantId", "IsPublic" },
                filter: "[IsPublic] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_ModifiedById",
                table: "Roles",
                columns: new[] { "TenantId", "ModifiedById" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_LocalUsers_Custodies_TenantId_AgentId",
                table: "LocalUsers",
                columns: new[] { "TenantId", "AgentId" },
                principalTable: "Custodies",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Custodies_LocalUsers_TenantId_CreatedById",
                table: "Custodies");

            migrationBuilder.DropForeignKey(
                name: "FK_Custodies_LocalUsers_TenantId_ModifiedById",
                table: "Custodies");

            migrationBuilder.DropTable(
                name: "MeasurementUnits");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RoleMemberships");

            migrationBuilder.DropTable(
                name: "Views");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "LocalUsers");

            migrationBuilder.DropTable(
                name: "Custodies");
        }
    }
}
