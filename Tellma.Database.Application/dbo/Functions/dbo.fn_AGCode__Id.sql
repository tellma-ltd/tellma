CREATE FUNCTION [dbo].[fn_AGCode__Id]
(
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.[Contracts]
		WHERE [Code] = @Code
	)
END;