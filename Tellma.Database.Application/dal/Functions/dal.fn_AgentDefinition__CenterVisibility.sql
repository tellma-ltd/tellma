CREATE FUNCTION [dal].[fn_AgentDefinition__CenterVisibility] (
	@Id INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN 	(
		SELECT [CenterVisibility] FROM dbo.AgentDefinitions
		WHERE [Id] = @Id
	)
END