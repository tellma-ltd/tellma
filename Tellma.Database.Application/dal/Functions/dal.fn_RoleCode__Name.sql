CREATE FUNCTION [dal].[fn_RoleCode__Name](@Id NVARCHAR (50))
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Roles]
		WHERE [Code] = @Id
	)
END