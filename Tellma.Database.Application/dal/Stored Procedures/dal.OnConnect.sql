CREATE PROCEDURE [dal].[OnConnect]
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255),
	@IsServiceAccount BIT = 0,
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
        @UserSettingsVersion UNIQUEIDENTIFIER,
        @Enforce2faOnLocalAccounts BIT,
        @EnforceNoExternalAccounts BIT;
        
    -- Get the User Info
    IF (@ExternalUserId IS NOT NULL OR @UserEmail IS NOT NULL)
    BEGIN
        SELECT
            @UserId				= [Id],
            @ExternalId			= [ExternalId],
            @Email				= [Email],
            @PermissionsVersion = [PermissionsVersion],
            @UserSettingsVersion = [UserSettingsVersion]
        FROM [dbo].[Users]
        WHERE [IsActive] = 1
	    AND [IsService] = @IsServiceAccount AND ([ExternalId] = @ExternalUserId OR ([IsService] = 0 AND [Email] = @UserEmail));
    END;

    -- Get Tenant Info
    SELECT
		@SettingsVersion    = [SettingsVersion],
        @DefinitionsVersion = [DefinitionsVersion],
        @Enforce2faOnLocalAccounts = [Enforce2faOnLocalAccounts],   
        @EnforceNoExternalAccounts = [EnforceNoExternalAccounts] 
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
        @DefinitionsVersion AS [DefinitionsVersion],
        @Enforce2faOnLocalAccounts AS [Enforce2faOnLocalAccounts],
        @EnforceNoExternalAccounts AS [EnforceNoExternalAccounts];
END;