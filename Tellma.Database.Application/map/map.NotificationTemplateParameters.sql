CREATE FUNCTION [map].[NotificationTemplateParameters] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[NotificationTemplateParameters]
);
