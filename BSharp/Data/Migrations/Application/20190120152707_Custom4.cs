using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom4 : Migration
    {
        protected static string RoleForSaveList = nameof(RoleForSaveList);
        protected static string PermissionForSaveList = nameof(PermissionForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            // DTOs for save
            builder.CreateUserDefinedTableType(
                name: RoleForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    Name = udt.Column<string>(nullable: true, maxLength: 255),
                    Name2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Code = udt.Column<string>(nullable: true, maxLength: 255),
                    IsPublic = udt.Column<bool>(nullable: true)
                }
            );

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
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(name: RoleForSaveList);
            builder.DropUserDefinedTableType(name: PermissionForSaveList);
        }
    }
}
