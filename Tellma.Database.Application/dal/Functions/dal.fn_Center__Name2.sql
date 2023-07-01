CREATE FUNCTION [dal].[fn_Center__Name2](
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Centers]
		WHERE [Id] = @Id
	)
END
GO