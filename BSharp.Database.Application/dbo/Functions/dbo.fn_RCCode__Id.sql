CREATE FUNCTION [dbo].[fn_RCCode__Id]
(
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.ResourceClassifications
		WHERE [Code] = @Code
	)
END;