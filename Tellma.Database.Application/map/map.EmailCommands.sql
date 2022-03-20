CREATE FUNCTION [map].[EmailCommands]()
RETURNS TABLE
AS
RETURN (
	SELECT *,
	(SELECT COUNT(*) FROM [dbo].[Emails] WHERE [State] > 2 AND [CommandId] = [Q].[Id]) As [Successes],
    (SELECT COUNT(*) FROM [dbo].[Emails] WHERE [State] < 0 AND [CommandId] = [Q].[Id]) As [Errors],
    (SELECT COUNT(*) FROM [dbo].[Emails] WHERE [CommandId] = [Q].[Id]) As [Total]
	FROM [dbo].[EmailCommands] As [Q]
);