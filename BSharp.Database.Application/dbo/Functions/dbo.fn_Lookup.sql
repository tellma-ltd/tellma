CREATE FUNCTION [dbo].[fn_Lookup]
(
	@LookupDefinitionId NVARCHAR(50),
	@Name NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Lookups
		WHERE LookupDefinitionId = @LookupDefinitionId
		AND [Name] = @Name
	)
END


