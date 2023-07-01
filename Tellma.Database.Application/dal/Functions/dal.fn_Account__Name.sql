CREATE FUNCTION [dal].[fn_Account__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Accounts]
		WHERE [Id] = @Id
	)
END
GO