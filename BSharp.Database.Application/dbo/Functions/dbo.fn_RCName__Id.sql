CREATE FUNCTION [dbo].[fn_RCName__Id]
(
	@Name NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.ResourceClassifications
		WHERE [Name] = @Name

	)
END


