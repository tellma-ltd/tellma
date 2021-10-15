CREATE FUNCTION [map].[NotificationTemplateSubscribers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[NotificationTemplateSubscribers]
);
