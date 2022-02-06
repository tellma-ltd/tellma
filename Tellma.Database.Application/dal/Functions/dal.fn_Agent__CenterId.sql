CREATE FUNCTION [dal].[fn_Agent__CenterId] (
	@Id INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [CenterId] FROM dbo.Agents
		WHERE [Id] = @Id
	)
END