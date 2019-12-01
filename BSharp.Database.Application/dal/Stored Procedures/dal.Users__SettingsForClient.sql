CREATE PROCEDURE [dal].[Users__SettingsForClient]
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Return the User Info
	SELECT 
		[U].[Id] AS [UserId], 
		[U].[Name], 
		[U].[Name2], 
		[U].[Name3], 
		[U].[ImageId], 
		[U].[UserSettingsVersion]
	FROM [dbo].[Users] AS [U]
	WHERE [U].[Id] = @UserId

	-- Return the Custom Settings
	SELECT [Key], [Value] FROM [dbo].[UserSettings]
	WHERE [UserId] = @UserId
