using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class IfrsNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ModifiedById",
                table: "MeasurementUnits",
                nullable: false,
                defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))",
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ModifiedAt",
                table: "MeasurementUnits",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<int>(
                name: "CreatedById",
                table: "MeasurementUnits",
                nullable: false,
                defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))",
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MeasurementUnits",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "MeasurementUnits",
                nullable: false,
                defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))",
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "IfrsConcepts",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))"),
                    IfrsType = table.Column<string>(maxLength: 255, nullable: false, defaultValue: "Regulatory"),
                    Label = table.Column<string>(maxLength: 1024, nullable: false),
                    Label2 = table.Column<string>(maxLength: 1024, nullable: true),
                    Label3 = table.Column<string>(maxLength: 1024, nullable: true),
                    Documentation = table.Column<string>(nullable: false),
                    Documentation2 = table.Column<string>(nullable: true),
                    Documentation3 = table.Column<string>(nullable: true),
                    EffectiveDate = table.Column<DateTime>(nullable: false, defaultValueSql: "'0001-01-01 00:00:00'"),
                    ExpiryDate = table.Column<DateTime>(nullable: false, defaultValueSql: "'9999-12-31 23:59:59'"),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedById = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))"),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedById = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IfrsConcepts", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_IfrsConcepts_LocalUsers_TenantId_CreatedById",
                        columns: x => new { x.TenantId, x.CreatedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IfrsConcepts_LocalUsers_TenantId_ModifiedById",
                        columns: x => new { x.TenantId, x.ModifiedById },
                        principalTable: "LocalUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IfrsNotes",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(nullable: false, defaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))"),
                    Node = table.Column<string>(nullable: true),
                    Level = table.Column<short>(maxLength: 255, nullable: false),
                    ParentNode = table.Column<string>(nullable: true),
                    IsAggregate = table.Column<bool>(nullable: false, defaultValue: true),
                    ForDebit = table.Column<bool>(nullable: false, defaultValue: true),
                    ForCredit = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IfrsNotes", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_IfrsNotes_IfrsConcepts_TenantId_Id",
                        columns: x => new { x.TenantId, x.Id },
                        principalTable: "IfrsConcepts",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IfrsConcepts_TenantId_CreatedById",
                table: "IfrsConcepts",
                columns: new[] { "TenantId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_IfrsConcepts_TenantId_ModifiedById",
                table: "IfrsConcepts",
                columns: new[] { "TenantId", "ModifiedById" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IfrsNotes");

            migrationBuilder.DropTable(
                name: "IfrsConcepts");

            migrationBuilder.AlterColumn<int>(
                name: "ModifiedById",
                table: "MeasurementUnits",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ModifiedAt",
                table: "MeasurementUnits",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedById",
                table: "MeasurementUnits",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MeasurementUnits",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "MeasurementUnits",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValueSql: "CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");
        }
    }
}
