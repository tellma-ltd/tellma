CREATE FUNCTION [dal].[fn_DocumentDefinitionCode__Id] (
	@Code NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM dbo.DocumentDefinitions
		WHERE [Code] = @Code
	)
END