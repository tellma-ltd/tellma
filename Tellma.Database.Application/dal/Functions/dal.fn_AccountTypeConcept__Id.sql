CREATE FUNCTION [dal].[fn_AccountTypeConcept__Id] (
	@Concept NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].AccountTypes
		WHERE [Concept] = @Concept
	)
END