CREATE FUNCTION [dal].[fn_Lookup] (
	@LookupDefinitionId INT,
	@Name NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Lookups
		WHERE [DefinitionId] = @LookupDefinitionId
		AND [Name] = @Name
	)
END