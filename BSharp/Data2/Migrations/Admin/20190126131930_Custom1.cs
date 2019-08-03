using Microsoft.EntityFrameworkCore.Migrations;
using BSharp.Services.Migrations;
using System;

namespace BSharp.Data.Migrations.Admin
{
    public partial class Custom1 : Migration
    {
        protected static string MValidationErrorList = nameof(MValidationErrorList);
        protected static string MIndexedIdList = nameof(MIndexedIdList);
        protected static string MCodeList = nameof(MCodeList);
        protected static string MIdList = nameof(MIdList);
        protected static string GlobalUserForSaveList = nameof(GlobalUserForSaveList);
        protected static string TenantMembershipForSaveList = nameof(TenantMembershipForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            // Shared user defined table types
            builder.CreateUserDefinedTableType(
                name: MValidationErrorList,
                columns: udt => new
                {
                    Key = udt.Column<string>(nullable: false, maxLength: 255),
                    ErrorName = udt.Column<string>(nullable: false, maxLength: 255),
                    Argument1 = udt.Column<string>(nullable: true, maxLength: 255),
                    Argument2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Argument3 = udt.Column<string>(nullable: true, maxLength: 255),
                    Argument4 = udt.Column<string>(nullable: true, maxLength: 255),
                    Argument5 = udt.Column<string>(nullable: true, maxLength: 255),
                }
            );

            builder.CreateUserDefinedTableType(
                name: MIndexedIdList,
                columns: udt => new
                {
                    Id = udt.Column<int>(nullable: false),
                    Index = udt.Column<int>(nullable: false),
                }
            );

            builder.CreateUserDefinedTableType(
                name: MCodeList,
                columns: udt => new
                {
                    Code = udt.Column<string>(nullable: false, maxLength: 255)
                }
            );

            builder.CreateUserDefinedTableType(
                name: MIdList,
                columns: udt => new
                {
                    Id = udt.Column<int>(nullable: false),
                }
            );

            //builder.CreateUserDefinedTableType(
            //    name: GlobalUserForSaveList,
            //    columns: udt => new
            //    {
            //        Index = udt.Column<int>(nullable: false),

            //        Id = udt.Column<int>(nullable: true),
            //        EntityState = udt.Column<string>(nullable: false, maxLength: 255),

            //        Email = udt.Column<string>(nullable: true, maxLength: 255),
            //    }
            //);


            //builder.CreateUserDefinedTableType(
            //    name: TenantMembershipForSaveList,
            //    columns: udt => new
            //    {
            //        Index = udt.Column<int>(nullable: false),
            //        HeaderIndex = udt.Column<int>(nullable: false),

            //        Id = udt.Column<int>(nullable: true),
            //        EntityState = udt.Column<string>(nullable: false, maxLength: 255),
            //    }
            //);

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
            builder.DropUserDefinedTableType(name: MValidationErrorList);
            builder.DropUserDefinedTableType(name: MIndexedIdList);
            builder.DropUserDefinedTableType(name: MCodeList);
            builder.DropUserDefinedTableType(name: MIdList);

            builder.DropTable("DistributedCache");
        }
    }
}
