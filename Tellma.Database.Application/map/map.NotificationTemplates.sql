CREATE FUNCTION [map].[NotificationTemplates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[NotificationTemplates]
);
