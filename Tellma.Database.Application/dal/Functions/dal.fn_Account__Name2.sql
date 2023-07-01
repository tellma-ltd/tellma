CREATE FUNCTION [dal].[fn_Account__Name2](
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Accounts]
		WHERE [Id] = @Id
	)
END
GO