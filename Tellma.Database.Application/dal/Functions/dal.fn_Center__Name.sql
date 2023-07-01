CREATE FUNCTION [dal].[fn_Center__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Centers]
		WHERE [Id] = @Id
	)
END
GO