CREATE FUNCTION [bll].[fn_RSD__CD](@Id INT)
RETURNS INT AS 
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.ResourceDefinitions
		WHERE [Code] = (
			SELECT [Code] FROM dbo.CustodyDefinitions WHERE [Id] = @Id
		)
	)
END