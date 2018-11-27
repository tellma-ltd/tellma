using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Localization
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoreTranslations",
                columns: table => new
                {
                    Tier = table.Column<string>(maxLength: 50, nullable: false),
                    Culture = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 450, nullable: false),
                    Value = table.Column<string>(maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreTranslations", x => new { x.Tier, x.Culture, x.Name });
                });

            migrationBuilder.Sql(
                @"CREATE TABLE [dbo].[DistributedCache](
	                [Id] [nvarchar](449) NOT NULL,
	                [Value] [varbinary](max) NOT NULL,
	                [ExpiresAtTime] [datetimeoffset](7) NOT NULL,
	                [SlidingExpirationInSeconds] [bigint] NULL,
	                [AbsoluteExpiration] [datetimeoffset](7) NULL,
                PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )
                )
                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [dbo].[DistributedCache]");

            migrationBuilder.DropTable(
                name: "CoreTranslations");
        }
    }
}
