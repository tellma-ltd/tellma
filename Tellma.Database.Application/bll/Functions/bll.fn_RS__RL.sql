CREATE FUNCTION [bll].[fn_RS__RL](@Id INT)
RETURNS INT AS 
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Resources
		WHERE [Code] = (
			SELECT [Code] FROM dbo.Relations WHERE [Id] = @Id
		)
		AND [DefinitionId] = (
			SELECT bll.fn_RSD__RLD([DefinitionId]) FROM dbo.Relations WHERE [Id] = @Id
		)
	)
END