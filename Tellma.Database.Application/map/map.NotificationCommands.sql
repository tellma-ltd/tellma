CREATE FUNCTION [map].[NotificationCommands]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[NotificationCommands]
);