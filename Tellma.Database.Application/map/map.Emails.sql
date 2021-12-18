CREATE FUNCTION [map].[Emails]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[CommandId],
		[To],
		[Cc],
		[Bcc],
		[Subject],
		[BodyBlobId],
		[State],
		[ErrorMessage],
		[StateSince],
		[DeliveredAt],
		[OpenedAt],
		[CreatedAt]
	FROM [dbo].[Emails]
);