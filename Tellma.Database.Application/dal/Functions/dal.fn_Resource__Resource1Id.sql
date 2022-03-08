CREATE FUNCTION [dal].[fn_Resource__Resource1Id] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Resource1Id] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END