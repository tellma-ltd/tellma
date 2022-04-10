CREATE FUNCTION [dal].[fn_EntryTypeConcept__Id] (
	@Concept NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].EntryTypes
		WHERE [Concept] = @Concept
	)
END