CREATE FUNCTION [dbo].[fi_Agents] ( -- SELECT * FROM dbo.fi_Agents(N'supplier', 0))
	@DefinitionId NVARCHAR (50) = NULL,
	@IsActive BIT = NULL
) RETURNS TABLE
AS
RETURN
	SELECT * FROM [dbo].[Agents]
	WHERE [DefinitionId] = @DefinitionId
	AND @IsActive IS NULL OR (IsActive = @IsActive);