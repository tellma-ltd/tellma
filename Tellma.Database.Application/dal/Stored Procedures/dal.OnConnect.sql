CREATE PROCEDURE [dal].[OnConnect]
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255),
	@SetLastActive BIT = 1
AS
BEGIN
    DECLARE 
        @UserId INT,
        @ExternalId NVARCHAR(450), 
        @Email NVARCHAR(255), 
        @SettingsVersion UNIQUEIDENTIFIER, 
        @PermissionsVersion UNIQUEIDENTIFIER,
        @DefinitionsVersion UNIQUEIDENTIFIER,
        @UserSettingsVersion UNIQUEIDENTIFIER;
        
    -- Get the User Info
    SELECT
        @UserId				= [Id],
        @ExternalId			= [ExternalId],
        @Email				= [Email],
        @PermissionsVersion = [PermissionsVersion],
        @UserSettingsVersion = [UserSettingsVersion]
    FROM [dbo].[Users]
    WHERE [IsActive] = 1
	AND ([ExternalId] = @ExternalUserId OR [Email] = @UserEmail);

    -- Get Tenant Info
    SELECT
		@SettingsVersion    = [SettingsVersion],
        @DefinitionsVersion = [DefinitionsVersion]
    FROM [dbo].[Settings];    

    -- Set LastAccess
    IF (@SetLastActive = 1 AND @UserId IS NOT NULL)
        UPDATE [dbo].[Users] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Return the user information
    SELECT 
		-- User Info
        @UserId AS [UserId], 
        @ExternalId AS [ExternalId], 
        @Email AS [Email], 
        @PermissionsVersion AS [PermissionsVersion],
        @UserSettingsVersion AS [UserSettingsVersion],
		-- Tenant Info
        @SettingsVersion AS [SettingsVersion],
        @DefinitionsVersion AS [DefinitionsVersion];
END;