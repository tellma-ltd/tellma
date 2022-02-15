CREATE FUNCTION [dal].[fn_Agent__Agent1Id] (
	@Id INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Agent1Id] FROM dbo.Agents
		WHERE [Id] = @Id
	)
END