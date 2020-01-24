CREATE FUNCTION [dbo].[fn_AGCode__Id]
(
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Agents
		WHERE [Code] = @Code
	)
END;