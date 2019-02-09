using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Application
{
    public partial class Custom2 : Migration
    {
        protected override void Up(MigrationBuilder builder)
        {
            builder.Sql(@"
    EXEC('CREATE FUNCTION [dbo].[fn_User__Language]()
    RETURNS INT
    AS
    BEGIN
	    DECLARE @Culture NVARCHAR(255) = CONVERT(NVARCHAR(255), SESSION_CONTEXT(N''Culture''));
	    DECLARE @NeutralCulture NVARCHAR(255) = CONVERT(NVARCHAR(255), SESSION_CONTEXT(N''NeutralCulture''));
	    DECLARE @TenantLanguage2 NVARCHAR(255);
        SELECT @TenantLanguage2 = SecondaryLanguageId FROM [dbo].[Settings];

	    RETURN CASE 
		    WHEN @TenantLanguage2 IN (@Culture, @NeutralCulture) THEN 2
		    ELSE 1
	    END;
    END;')
");

            builder.Sql(@"
    EXEC('CREATE FUNCTION dbo.[fn_IsNullOrEmpty](@Str2 NVARCHAR(MAX), @Str NVARCHAR(MAX))
    RETURNS NVARCHAR(MAX) AS
    BEGIN
        RETURN CASE 
            WHEN @Str2 IS NOT NULL AND LEN(@Str2) > 0 THEN @Str2
            ELSE @Str
        END;
    END;')
");
        }

        protected override void Down(MigrationBuilder builder)
        {
            builder.Sql(@"
    DROP FUNCTION [dbo].[fn_IsNullOrEmpty];
    DROP FUNCTION [dbo].[fn_User__Language];
");
        }
    }
}
