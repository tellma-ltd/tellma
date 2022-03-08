CREATE FUNCTION [dal].[fn_Resource__Resource2Id] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Resource2Id] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END