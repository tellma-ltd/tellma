CREATE FUNCTION [dal].[fn_Resource__DefinitionId] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [DefinitionId] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END