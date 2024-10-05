CREATE FUNCTION [dal].[fn_DocumentDefinition__State] (
	@Id INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN 	(
		SELECT [State] FROM dbo.DocumentDefinitions
		WHERE [Id] = @Id
	)
END
GO