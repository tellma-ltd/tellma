CREATE FUNCTION [dal].[fn_Agent__DefinitionId] (
	@Id INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
			SELECT [DefinitionId] FROM dbo.Agents WHERE Id = @Id
	)
END