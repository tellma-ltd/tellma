CREATE FUNCTION [map].[LineDefinitionStateReasons]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionId],
		[Index],
		[State],
		[Name],
		[Name2],
		[Name3],
		[IsActive],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionStateReasons]
);