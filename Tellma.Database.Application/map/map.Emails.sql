CREATE FUNCTION [map].[Emails]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[ToEmail],
		[Subject],
		[Body],
		[State],
		[ErrorMessage],
		[StateSince],
		[DeliveredAt],
		[OpenedAt],
		[CreatedAt]
	FROM [dbo].[Emails]
);