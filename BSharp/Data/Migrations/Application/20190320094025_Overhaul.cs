using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Overhaul : Migration
    {
        protected override void Up(MigrationBuilder builder)
        {
            builder.AddColumn<string>(
                name: "Mask",
                table: "Permissions",
                maxLength: 2048,
                nullable: true);

            builder.Sql($@"CREATE VIEW [dbo].[VW_MeasurementUnits] AS SELECT * FROM [dbo].[MeasurementUnits];");
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.Sql($@"DROP VIEW [dbo].[VW_MeasurementUnits];");

            builder.DropColumn(
                name: "Mask",
                table: "Permissions");
        }
    }
}
