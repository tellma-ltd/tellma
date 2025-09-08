CREATE FUNCTION [dal].[fn_RoleCode__Name2] (@Id NVARCHAR (50))
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Roles]
		WHERE [Code] = @Id
	)
END