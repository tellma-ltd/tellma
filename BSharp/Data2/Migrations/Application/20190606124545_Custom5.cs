using BSharp.Services.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom5 : Migration
    {
        protected static string ProductCategoryList = nameof(ProductCategoryList);
        protected static string Tree = nameof(Tree);

        protected override void Up(MigrationBuilder builder)
        {
            // Correct the column types with SQL
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [Node];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [Node] HIERARCHYID;");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [Level];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [Level] AS [Node].GetLevel();");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [ParentNode];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [ParentNode] AS [Node].GetAncestor(1);");

            builder.Sql(@"
CREATE UNIQUE INDEX [IX_ProductCategories__Node] ON [dbo].[ProductCategories]([TenantId], [Node]);
GO
CREATE INDEX [IX_ProductCategories__Level_Node] ON [dbo].[ProductCategories]([TenantId], [Level], [Node]);
GO");

            // Add the user defined table types
            builder.Sql(@"
CREATE TYPE [dbo].[ProductCategoryList] AS TABLE (
	[Index]				INT,

	[Id]				INT,
	[EntityState]		NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),

	[ParentIndex]		INT,
	[ParentId]			INT,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	PRIMARY KEY ([Index] ASC),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);");

        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropUserDefinedTableType(name: ProductCategoryList);

            builder.Sql(@"
DROP INDEX [IX_ProductCategories__Node]; 
DROP INDEX [IX_ProductCategories__Level_Node];
");

            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [ParentNode];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [ParentNode] NVARCHAR(MAX);");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [Level];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [Level] SMALLINT;");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [Node];");
            builder.Sql("ALTER TABLE [dbo].[ProductCategories] ADD [Node] NVARCHAR(MAX);");
        }
    }
}
