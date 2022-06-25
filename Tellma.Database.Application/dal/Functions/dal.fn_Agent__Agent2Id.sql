CREATE FUNCTION [dal].[fn_Agent__Agent2Id] (
	@Id INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Agent2Id] FROM dbo.Agents
		WHERE [Id] = @Id
	)
END