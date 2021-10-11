CREATE FUNCTION [map].[LineDefinitionColumns]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionId],
		[Index],
		[ColumnName],
		[EntryIndex],
		[Label],
		[Label2],
		[Label3],
		[Filter],
		[InheritsFromHeader],
		[VisibleState],
		[RequiredState],
		[ReadOnlyState],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionColumns]
);