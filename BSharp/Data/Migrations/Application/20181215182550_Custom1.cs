using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom1 : Migration
    {
        protected static string ValidationErrorList = nameof(ValidationErrorList);
        protected static string IndexedIdList = nameof(IndexedIdList);
        protected static string CodeList = nameof(CodeList);
        protected static string IdList = nameof(IdList);
        protected static string MeasurementUnitForSaveList = nameof(MeasurementUnitForSaveList);

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
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(name: ValidationErrorList);
            builder.DropUserDefinedTableType(name: IndexedIdList);
            builder.DropUserDefinedTableType(name: CodeList);
            builder.DropUserDefinedTableType(name: IdList);
            builder.DropUserDefinedTableType(name: MeasurementUnitForSaveList);
        }
    }
}
