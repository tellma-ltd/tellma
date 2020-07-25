CREATE FUNCTION [dbo].[fi_Relations] ( -- SELECT * FROM dbo.[fi_Relations](N'supplier', 0))
	@DefinitionId NVARCHAR (50) = NULL,
	@IsActive BIT = NULL
) RETURNS TABLE
AS
RETURN
	SELECT R.* FROM [dbo].[Relations] R
	JOIN dbo.[RelationDefinitions] RD ON R.[DefinitionId] = RD.[Id]
	WHERE RD.Code = @DefinitionId
	AND @IsActive IS NULL OR (R.IsActive = @IsActive);