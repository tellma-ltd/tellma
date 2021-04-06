CREATE FUNCTION [bll].[fn_RS__C](@Id INT)
RETURNS INT AS 
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Resources
		WHERE [Code] = (
			SELECT [Code] FROM dbo.Custodies WHERE [Id] = @Id
		)
		AND [DefinitionId] = (
			SELECT bll.fn_RSD__CD([DefinitionId]) FROM dbo.Custodies WHERE [Id] = @Id
		)
	)
END