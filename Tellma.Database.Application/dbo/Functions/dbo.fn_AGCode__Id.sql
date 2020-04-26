CREATE FUNCTION [dbo].[fn_AGCode__Id]
(
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.[Relations]
		WHERE [Code] = @Code
	)
END;