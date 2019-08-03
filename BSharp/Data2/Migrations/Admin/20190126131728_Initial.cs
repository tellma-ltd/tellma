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
                    Symbol = table.Column<string>(maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(maxLength: 450, nullable: true),
                    Email = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalUsers", x => x.Id);
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
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(maxLength: 2048, nullable: false),
                    Tier = table.Column<string>(maxLength: 255, nullable: false)
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
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Name2 = table.Column<string>(maxLength: 255, nullable: true),
                    Code = table.Column<string>(maxLength: 255, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "TenantMemberships",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMemberships", x => new { x.UserId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_TenantMemberships_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantMemberships_GlobalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GlobalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cultures",
                columns: new[] { "Id", "IsActive", "Name", "Symbol" },
                values: new object[] { "en", true, "English", "En" });

            migrationBuilder.InsertData(
                table: "Cultures",
                columns: new[] { "Id", "IsActive", "Name", "Symbol" },
                values: new object[] { "ar", true, "العربية", "ع" });

            migrationBuilder.InsertData(
                table: "Shards",
                columns: new[] { "Id", "ConnectionString", "Name" },
                values: new object[] { 1, "<ShardManager>", "Shard Manager" });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUsers_Email",
                table: "GlobalUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUsers_ExternalId",
                table: "GlobalUsers",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMemberships_TenantId",
                table: "TenantMemberships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ShardId",
                table: "Tenants",
                column: "ShardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantMemberships");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "GlobalUsers");

            migrationBuilder.DropTable(
                name: "Cultures");

            migrationBuilder.DropTable(
                name: "Shards");
        }
    }
}
