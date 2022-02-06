CREATE FUNCTION [dal].[fn_AccountTypeConcept__Node] (
	@Concept NVARCHAR (255)
)
RETURNS HIERARCHYID
AS
BEGIN
	RETURN 	(
		SELECT [Node] FROM [dbo].AccountTypes
		WHERE [Concept] = @Concept
	)
END