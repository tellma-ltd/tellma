using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Overhaul2 : Migration
    {
        protected static string PermissionForSaveList = nameof(PermissionForSaveList);
        protected static string RequiredSignatureForSaveList = nameof(RequiredSignatureForSaveList);

        protected override void Up(MigrationBuilder builder)
        {
            // Views are hardcoded
            builder.Sql($@"CREATE VIEW [dbo].[VW_Views] AS SELECT
 V.[Id], 
 NULL AS [Name], 
 NULL AS [Name2], 
 V.[ResourceName], 
 V.[Id] AS [Code], 
 CASE WHEN V.[Id] = 'all' THEN CAST(1 AS BIT) ELSE IsNULL(T.[IsActive], CAST(0 AS BIT)) END AS [IsActive], 
 V.[AllowedPermissionLevels], 
 CAST(V.[SupportsCriteria] AS BIT) AS [SupportsCriteria], 
 CAST(V.[SupportsMask] AS BIT) AS [SupportsMask]
FROM 
  (
  VALUES
    ('all', 'View_All', 'ReadUpdate', 0, 0),
    ('measurement-units', 'MeasurementUnits', 'ReadUpdate', 1, 1),
    ('roles', 'Roles', 'ReadUpdate',  1, 1),
    ('local-users', 'Users', 'ReadUpdate',  1, 1),
    ('views', 'Views', 'ReadUpdate',  1, 1),
    ('individuals', 'Individuals', 'ReadUpdate', 1, 1),
    ('organizations', 'Organizations', 'ReadUpdate', 1, 1),
	('settings', 'Settings', 'ReadUpdate', 0, 0)
  ) 
AS V ([Id], [ResourceName], [AllowedPermissionLevels], [SupportsCriteria], [SupportsMask])
LEFT JOIN [dbo].[Views] AS T ON V.Id = T.Id;");

            builder.Sql($@"CREATE VIEW [dbo].[VW_LocalUsers] AS SELECT * FROM [dbo].[LocalUsers];");
            builder.Sql($@"CREATE VIEW [dbo].[VW_Agents] AS SELECT * FROM [dbo].[Custodies] WHERE [CustodyType] = 'Agent';");
            builder.Sql($@"CREATE VIEW [dbo].[VW_Custodies] AS SELECT * FROM [dbo].[Custodies]");
            builder.Sql($@"CREATE VIEW [dbo].[VW_Roles] AS SELECT * FROM [dbo].[Roles]");
            builder.Sql($@"CREATE VIEW [dbo].[VW_Permissions] AS SELECT * FROM [dbo].[Permissions] WHERE [Level] <> 'Sign'");
            builder.Sql($@"CREATE VIEW [dbo].[VW_RequiredSignatures] AS SELECT * FROM [dbo].[Permissions] WHERE [Level] = 'Sign'");
            builder.Sql($@"CREATE VIEW [dbo].[VW_RoleMemberships] AS SELECT * FROM [dbo].[RoleMemberships]");

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

            builder.Sql($@"DROP VIEW [dbo].[VW_RoleMemberships];");
            builder.Sql($@"DROP VIEW [dbo].[VW_RequiredSignatures];");
            builder.Sql($@"DROP VIEW [dbo].[VW_Permissions];");
            builder.Sql($@"DROP VIEW [dbo].[VW_Roles];");
            builder.Sql($@"DROP VIEW [dbo].[VW_Custodies];");
            builder.Sql($@"DROP VIEW [dbo].[VW_Agents];");
            builder.Sql($@"DROP VIEW [dbo].[VW_LocalUsers];");
            builder.Sql($@"DROP VIEW [dbo].[VW_Views];");
        }
    }
}
