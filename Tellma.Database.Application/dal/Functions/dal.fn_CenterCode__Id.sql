CREATE FUNCTION [dal].[fn_CenterCode__Id] (
	@Code	NVARCHAR (50)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE [Code] = @Code
	)
END;