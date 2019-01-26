using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom1 : Migration
    {
        protected static string ValidationErrorList = nameof(ValidationErrorList);
        protected static string IndexedIdList = nameof(IndexedIdList);
        protected static string CodeList = nameof(CodeList);
        protected static string IdList = nameof(IdList);
        protected static string MeasurementUnitForSaveList = nameof(MeasurementUnitForSaveList);
        protected static string AgentForSaveList = nameof(AgentForSaveList);
        protected static string RoleForSaveList = nameof(RoleForSaveList);
        protected static string PermissionForSaveList = nameof(PermissionForSaveList);
        protected static string LocalUserForSaveList = nameof(LocalUserForSaveList);
        protected static string RoleMembershipForSaveList = nameof(RoleMembershipForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            // Shared user defined table types
            builder.CreateUserDefinedTableType(
                name: ValidationErrorList,
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
                name: IndexedIdList,
                columns: udt => new
                {
                    Id = udt.Column<int>(nullable: false),
                    Index = udt.Column<int>(nullable: false),
                }
            );

            builder.CreateUserDefinedTableType(
                name: CodeList,
                columns: udt => new
                {
                    Code = udt.Column<string>(nullable: false, maxLength: 255)
                }
            );

            builder.CreateUserDefinedTableType(
                name: IdList,
                columns: udt => new
                {
                    Id = udt.Column<int>(nullable: false),
                }
            );

            // DTOs for save
            builder.CreateUserDefinedTableType(
                name: MeasurementUnitForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    Name = udt.Column<string>(nullable: true, maxLength: 255),
                    Name2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Code = udt.Column<string>(nullable: true, maxLength: 255),
                    UnitType = udt.Column<string>(nullable: true, maxLength: 255),
                    UnitAmount = udt.Column<double>(nullable: true),
                    BaseAmount = udt.Column<double>(nullable: true),
                }
            );

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

            builder.CreateUserDefinedTableType(
                name: LocalUserForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    Name = udt.Column<string>(nullable: true, maxLength: 255),
                    Name2 = udt.Column<string>(nullable: true, maxLength: 255),
                    Email = udt.Column<string>(nullable: true, maxLength: 255),
                    ExternalId = udt.Column<string>(nullable: true, maxLength: 450),
                    AgentId = udt.Column<int>(nullable: true)
                }
            );

            builder.CreateUserDefinedTableType(
                name: RoleMembershipForSaveList,
                columns: udt => new
                {
                    Index = udt.Column<int>(nullable: false),
                    HeaderIndex = udt.Column<int>(nullable: false),

                    Id = udt.Column<int>(nullable: true),
                    EntityState = udt.Column<string>(nullable: false, maxLength: 255),

                    UserId = udt.Column<int>(nullable: true),
                    RoleId = udt.Column<int>(nullable: true),
                    Memo = udt.Column<string>(nullable: true, maxLength: 255)
                }
            );
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(name: ValidationErrorList);
            builder.DropUserDefinedTableType(name: IndexedIdList);
            builder.DropUserDefinedTableType(name: CodeList);
            builder.DropUserDefinedTableType(name: IdList);
            builder.DropUserDefinedTableType(name: MeasurementUnitForSaveList);
            builder.DropUserDefinedTableType(name: AgentForSaveList);
            builder.DropUserDefinedTableType(name: RoleForSaveList);
            builder.DropUserDefinedTableType(name: PermissionForSaveList);
            builder.DropUserDefinedTableType(name: LocalUserForSaveList);
        }
    }
}
