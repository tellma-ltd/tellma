CREATE FUNCTION [dal].[fn_RoleCode__Name3] (
	@Id NVARCHAR (50)
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name3] FROM [dbo].[Roles]
		WHERE [Code] = @Id
	)
END