CREATE PROCEDURE [dal].[OnConnect]
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255),
    @IsServiceAccount BIT = 0,
    @SetLastActive BIT = 1
AS
BEGIN
    -- Get the User Id
    DECLARE 
        @UserId INT, 
        @ExternalId NVARCHAR(450), 
        @Email NVARCHAR(255),
        @PermissionsVersion UNIQUEIDENTIFIER,
        @UserSettingsVersion UNIQUEIDENTIFIER;

    SELECT
        @UserId = [Id],
        @ExternalId = [ExternalId],
        @Email = [Email],
        @PermissionsVersion = [PermissionsVersion],
        @UserSettingsVersion = [UserSettingsVersion]
    FROM [dbo].[AdminUsers] 
    WHERE [IsActive] = 1
	AND [IsService] = @IsServiceAccount AND ([ExternalId] = @ExternalUserId OR ([IsService] = 0 AND [Email] = @UserEmail));
            
    -- Set LastAccess
    IF (@SetLastActive = 1 AND @UserId IS NOT NULL)
        UPDATE [dbo].[AdminUsers] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Return the user and tenant information
    SELECT 
		-- Global User Info
        @UserId AS [UserId], 
        @ExternalId AS [ExternalId], 
        @Email AS [Email],
        @PermissionsVersion AS [PermissionsVersion],
        @UserSettingsVersion AS [UserSettingsVersion]
END;
