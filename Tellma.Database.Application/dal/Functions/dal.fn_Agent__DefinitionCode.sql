CREATE FUNCTION [dal].[fn_Agent__DefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN (
		SELECT [Code] FROM dbo.AgentDefinitions
		WHERE [Id] = dal.fn_Agent__DefinitionId(@Id)
	)
END