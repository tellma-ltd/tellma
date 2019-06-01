using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom4 : Migration
    {
        protected override void Up(MigrationBuilder builder)
        {

            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [Node];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [Node] HIERARCHYID;");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [Level];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [Level] AS [Node].GetLevel();");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [ParentNode];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [ParentNode] AS [Node].GetAncestor(1);");
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [ParentNode];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [ParentNode] NVARCHAR(MAX);");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [Level];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [Level] SMALLINT;");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] DROP COLUMN [Node];");
            builder.Sql("ALTER TABLE [dbo].[IfrsNotes] ADD [Node] NVARCHAR(MAX);");
        }
    }
}
