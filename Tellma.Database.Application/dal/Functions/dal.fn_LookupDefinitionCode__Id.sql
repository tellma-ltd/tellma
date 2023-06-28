CREATE FUNCTION [dal].[fn_LookupDefinitionCode__Id] (
	@Code NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM dbo.LookupDefinitions
		WHERE [Code] = @Code
	)
END