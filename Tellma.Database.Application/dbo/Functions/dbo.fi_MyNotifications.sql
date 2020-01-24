CREATE FUNCTION [dbo].[fi_MyNotifications]()
RETURNS TABLE
AS
RETURN
	SELECT *
	FROM [dbo].Notifications
	WHERE RecipientId = CONVERT(INT, SESSION_CONTEXT(N'UserId'));