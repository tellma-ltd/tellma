CREATE PROCEDURE [dal].[UserSettings__Load]
    @UserId INT
AS
BEGIN
	-- Return the User Info
	SELECT 
		[U].[Id], 
		[U].[Name],
		[U].[UserSettingsVersion]
	FROM [dbo].[AdminUsers] AS [U]
	WHERE [U].[Id] = @UserId

	-- Return the Custom Settings
	SELECT [Key], [Value] FROM [dbo].[AdminUserSettings]
	WHERE [AdminUserId] = @UserId;
END;
