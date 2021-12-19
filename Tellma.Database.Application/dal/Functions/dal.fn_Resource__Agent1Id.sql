CREATE FUNCTION [dal].[fn_Resource__Agent1Id] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Agent1Id] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END