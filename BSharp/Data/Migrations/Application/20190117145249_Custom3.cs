using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom3 : Migration
    {
        protected static string AgentForSaveList = nameof(AgentForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            // DTOs for save
            builder.DropUserDefinedTableType(name: AgentForSaveList);
            builder.CreateUserDefinedTableType(
                name: AgentForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    Name = udt.Column<string>(nullable: true, maxLength: 255),
                    Name2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Code = udt.Column<string>(nullable: true, maxLength: 255),
                    Address = udt.Column<string>(nullable: true, maxLength: 1024),
                    BirthDateTime = udt.Column<DateTimeOffset>(nullable: true),
                    IsRelated = udt.Column<bool>(nullable: true),
                    TaxIdentificationNumber = udt.Column<string>(nullable: true, maxLength: 255),
                    Title = udt.Column<string>(nullable: true, maxLength: 255),
                    Title2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Gender = udt.Column<char>(nullable: true)
                }
            );
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(name: AgentForSaveList);
            builder.CreateUserDefinedTableType(
                name: AgentForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    Name = udt.Column<string>(nullable: true, maxLength: 255),
                    Name2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Code = udt.Column<string>(nullable: true, maxLength: 255),
                    Address = udt.Column<string>(nullable: true, maxLength: 1024),
                    BirthDateTime = udt.Column<DateTimeOffset>(nullable: true),
                    IsRelated = udt.Column<bool>(nullable: true),
                    UserId = udt.Column<string>(nullable: true, maxLength: 450),
                    TaxIdentificationNumber = udt.Column<string>(nullable: true, maxLength: 255),
                    Title = udt.Column<string>(nullable: true, maxLength: 255),
                    Title2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Gender = udt.Column<char>(nullable: true)
                }
            );
        }
    }
}
