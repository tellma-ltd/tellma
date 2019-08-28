CREATE PROCEDURE [dal].[Users__SettingsForClient]
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Return the User Info
	SELECT 
		[U].[Id] AS [UserId], 
		[A].[Name], 
		[A].[Name2], 
		[A].[Name3], 
		[A].[ImageId], 
		[U].[UserSettingsVersion]
	FROM [dbo].[Users] AS [U] JOIN [dbo].[Agents] AS [A] ON [U].[Id] = [A].[Id];

	-- Return the Custom Settings
	SELECT [Key], [Value] FROM [dbo].[UserSettings]
	WHERE [UserId] = @UserId
