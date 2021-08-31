CREATE FUNCTION [map].[LineDefinitionEntries]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionId],
		[Index],
		[Direction],
		[ParentAccountTypeId],
		[EntryTypeId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionEntries]
);
