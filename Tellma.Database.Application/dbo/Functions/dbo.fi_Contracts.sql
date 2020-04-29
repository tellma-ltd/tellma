CREATE FUNCTION [dbo].[fi_Contracts] ( -- SELECT * FROM dbo.fi_Contracts(N'supplier', 0))
	@DefinitionId NVARCHAR (50) = NULL,
	@IsActive BIT = NULL
) RETURNS TABLE
AS
RETURN
	SELECT R.* FROM [dbo].[Contracts] R
	JOIN dbo.[ContractDefinitions] RD ON R.[DefinitionId] = RD.[Id]
	WHERE RD.Code = @DefinitionId
	AND @IsActive IS NULL OR (R.IsActive = @IsActive);