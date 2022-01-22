CREATE PROCEDURE [dal].[UserSettings__Load]
	@UserId INT
AS
	-- Return the User Info
	SELECT 
		[U].[Id], 
		[U].[Name], 
		[U].[Name2], 
		[U].[Name3], 
		[U].[Email], 
		[U].[ImageId], 
		[U].[PreferredLanguage], 
		[U].[PreferredCalendar],
		[U].[UserSettingsVersion]
	FROM [dbo].[Users] AS [U]
	WHERE [U].[Id] = @UserId

	-- Return the Custom Settings
	SELECT [Key], [Value] FROM [dbo].[UserSettings]
	WHERE [UserId] = @UserId

