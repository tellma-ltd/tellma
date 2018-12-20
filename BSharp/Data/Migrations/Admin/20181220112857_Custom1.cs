using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BSharp.Data.Migrations.Admin
{
    public partial class Custom1 : Migration
    {
        protected override void Up(MigrationBuilder builder)
        {
            builder.CreateTable(
                name: "DistributedCache",
                columns: udt => new
                {
                    Id = udt.Column<string>(nullable: false, maxLength: 499),
                    Value = udt.Column<byte[]>(nullable: false),
                    ExpiresAtTime = udt.Column<DateTimeOffset>(nullable: false),
                    SlidingExpirationInSeconds = udt.Column<long>(nullable: true),
                    AbsoluteExpiration = udt.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedCache", x => x.Id);
                }
            );
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropTable("DistributedCache");
        }
    }
}
