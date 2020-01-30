CREATE FUNCTION [dbo].[fn_ATCode__Id] (
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.[AccountTypes]
		WHERE [Code] = @Code
	)
END;