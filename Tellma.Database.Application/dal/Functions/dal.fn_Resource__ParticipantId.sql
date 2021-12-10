CREATE FUNCTION [dal].[fn_Resource__ParticipantId] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [ParticipantId] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END