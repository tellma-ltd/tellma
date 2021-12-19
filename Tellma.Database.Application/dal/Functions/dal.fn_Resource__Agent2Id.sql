CREATE FUNCTION [dal].[fn_Resource__Agent2Id] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Agent2Id] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END