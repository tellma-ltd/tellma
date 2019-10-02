CREATE FUNCTION [dbo].[fn_ResourceName__Id] (
	@Name NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].Resources
		WHERE [Name] = @Name
	)
END
