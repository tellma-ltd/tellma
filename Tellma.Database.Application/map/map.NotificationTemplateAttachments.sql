CREATE FUNCTION [map].[NotificationTemplateAttachments] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[NotificationTemplateAttachments]
);
