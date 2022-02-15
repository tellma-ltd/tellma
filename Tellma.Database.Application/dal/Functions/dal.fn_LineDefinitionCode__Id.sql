CREATE FUNCTION [dal].[fn_LineDefinitionCode__Id] (
	@Code NVARCHAR (100)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [Code] = @Code
	)
END