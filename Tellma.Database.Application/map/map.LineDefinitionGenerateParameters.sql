CREATE FUNCTION [map].[LineDefinitionGenerateParameters]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionId],
		[Index],
		[Key],
		[Label],
		[Label2],
		[Label3],
		[Visibility],
		[Control],
		[ControlOptions],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionGenerateParameters]
);