CREATE FUNCTION [dbo].[fi_MyNotifications]()
RETURNS TABLE
AS
RETURN
	SELECT *
	FROM [dbo].Notifications
	WHERE RecipientId = (
		SELECT [AgentId]
		FROM dbo.[Users] 
		WHERE [Id] = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
	);