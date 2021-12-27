CREATE FUNCTION [map].[NotificationCommands]()
RETURNS TABLE
AS
RETURN (
	SELECT *,
	(SELECT COUNT(*) FROM [dbo].[Emails] WHERE [State] > 2 AND [CommandId] = [Q].[Id]) As [EmailSuccesses],
    (SELECT COUNT(*) FROM [dbo].[Emails] WHERE [State] < 0 AND [CommandId] = [Q].[Id]) As [EmailErrors],
    (SELECT COUNT(*) FROM [dbo].[Emails] WHERE [CommandId] = [Q].[Id]) As [EmailTotal],
	(SELECT COUNT(*) FROM [dbo].[Messages] WHERE [State] > 2 AND [CommandId] = [Q].[Id]) As [SmsSuccesses],
    (SELECT COUNT(*) FROM [dbo].[Messages] WHERE [State] < 0 AND [CommandId] = [Q].[Id]) As [SmsErrors],
    (SELECT COUNT(*) FROM [dbo].[Messages] WHERE [CommandId] = [Q].[Id]) As [SmsTotal]
	FROM [dbo].[NotificationCommands] As [Q]
);