CREATE FUNCTION [dal].[fn_LineDefinition__Code]
(
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[LineDefinitions]
		WHERE [Id] = @Id
	)
END
GO