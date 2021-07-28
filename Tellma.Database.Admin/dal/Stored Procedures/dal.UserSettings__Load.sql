CREATE PROCEDURE [dal].[UserSettings__Load]
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Return the User Info
	SELECT 
		[U].[Id], 
		[U].[Name],
		[U].[UserSettingsVersion]
	FROM [dbo].[AdminUsers] AS [U]
	WHERE [U].[Id] = @UserId

	-- Return the Custom Settings
	SELECT [Key], [Value] FROM [dbo].[AdminUserSettings]
	WHERE [AdminUserId] = @UserId
