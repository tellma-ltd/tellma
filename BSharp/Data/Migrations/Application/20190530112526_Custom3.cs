using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom3 : Migration
    {
        protected static string PermissionForSaveList = nameof(PermissionForSaveList);
        protected static string RequiredSignatureForSaveList = nameof(RequiredSignatureForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(PermissionForSaveList);
            builder.CreateUserDefinedTableType(
                name: PermissionForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),
                    HeaderIndex = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    ViewId = udt.Column<string>(nullable: true, maxLength: 255),
                    RoleId = udt.Column<int>(nullable: true, maxLength: 255),
                    Level = udt.Column<string>(nullable: true, maxLength: 255),
                    Criteria = udt.Column<string>(nullable: true, maxLength: 1024),
                    Mask = udt.Column<string>(nullable: true, maxLength: 2048),
                    Memo = udt.Column<string>(nullable: true, maxLength: 255)
                }
            );

            builder.CreateUserDefinedTableType(
                name: RequiredSignatureForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),
                    HeaderIndex = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    ViewId = udt.Column<string>(nullable: true, maxLength: 255),
                    RoleId = udt.Column<int>(nullable: true, maxLength: 255),
                    Criteria = udt.Column<string>(nullable: true, maxLength: 1024),
                    Memo = udt.Column<string>(nullable: true, maxLength: 255)
                }
            );
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(PermissionForSaveList);
            builder.CreateUserDefinedTableType(
                name: PermissionForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),
                    HeaderIndex = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    ViewId = udt.Column<string>(nullable: true, maxLength: 255),
                    RoleId = udt.Column<int>(nullable: true, maxLength: 255),
                    Level = udt.Column<string>(nullable: true, maxLength: 255),
                    Criteria = udt.Column<string>(nullable: true, maxLength: 1024),
                    Memo = udt.Column<string>(nullable: true, maxLength: 255)
                }
            );

            builder.DropUserDefinedTableType(RequiredSignatureForSaveList);
        }
    }
}
