CREATE FUNCTION [dal].[fn_EntryTypeConcept__Node] (
	@Concept NVARCHAR (255)
)
RETURNS HIERARCHYID
AS
BEGIN
	RETURN 	(
		SELECT [Node] FROM [dbo].EntryTypes
		WHERE [Concept] = @Concept
	)
END