using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class AddedSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    TenantId = table.Column<int>(nullable: false),
                    ShortCompanyName = table.Column<string>(maxLength: 255, nullable: false),
                    ShortCompanyName2 = table.Column<string>(maxLength: 255, nullable: true),
                    PrimaryLanguageId = table.Column<string>(maxLength: 255, nullable: false),
                    PrimaryLanguageSymbol = table.Column<string>(maxLength: 255, nullable: true),
                    SecondaryLanguageId = table.Column<string>(maxLength: 255, nullable: true),
                    SecondaryLanguageSymbol = table.Column<string>(maxLength: 255, nullable: true),
                    BrandColor = table.Column<string>(maxLength: 255, nullable: true),
                    ViewsAndSpecsVersion = table.Column<Guid>(nullable: false, defaultValue: new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd")),
                    SettingsVersion = table.Column<Guid>(nullable: false, defaultValue: new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd")),
                    ProvisionedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedById = table.Column<int>(nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.TenantId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
