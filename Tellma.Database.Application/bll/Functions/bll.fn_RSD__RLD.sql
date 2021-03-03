CREATE FUNCTION [bll].[fn_RSD__RLD](@Id INT)
RETURNS INT AS 
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.ResourceDefinitions
		WHERE [Code] = (
			SELECT [Code] FROM dbo.RelationDefinitions WHERE [Id] = @Id
		)
	)
END