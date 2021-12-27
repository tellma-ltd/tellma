CREATE FUNCTION [map].[MessageCommands]()
RETURNS TABLE
AS
RETURN (
	SELECT *,
	(SELECT COUNT(*) FROM [dbo].[SmsMessages] WHERE [State] > 2 AND [CommandId] = [Q].[Id]) As [Successes],
    (SELECT COUNT(*) FROM [dbo].[SmsMessages] WHERE [State] < 0 AND [CommandId] = [Q].[Id]) As [Errors],
    (SELECT COUNT(*) FROM [dbo].[SmsMessages] WHERE [CommandId] = [Q].[Id]) As [Total]
	FROM [dbo].[MessageCommands] As [Q]
);