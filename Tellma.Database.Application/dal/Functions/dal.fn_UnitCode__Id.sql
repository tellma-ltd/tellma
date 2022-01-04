CREATE FUNCTION [dal].[fn_UnitCode__Id] (
	@Code NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].[Units]
		WHERE [Code] = @Code
	)
END