using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Admin
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cultures",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    ConnectionString = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    CultureId = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 450, nullable: false),
                    Tier = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => new { x.CultureId, x.Name });
                    table.ForeignKey(
                        name: "FK_Translations_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ShardId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Shards_ShardId",
                        column: x => x.ShardId,
                        principalTable: "Shards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cultures",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[] { "en", true, "English" });

            migrationBuilder.InsertData(
                table: "Cultures",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[] { "ar", true, "العربية" });

            migrationBuilder.InsertData(
                table: "Shards",
                columns: new[] { "Id", "ConnectionString", "Name" },
                values: new object[] { 1, "<ShardManager>", "Shard Manager" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ShardId",
                table: "Tenants",
                column: "ShardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "Shards");

            migrationBuilder.DropTable(
                name: "Cultures");
        }
    }
}
