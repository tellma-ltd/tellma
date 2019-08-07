CREATE PROCEDURE [dal].[OnConnect]
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50),
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255)
AS
    -- Set the global values of the session context
    EXEC [sys].[sp_set_session_context] @key = N'Culture', @value = @Culture;
    EXEC [sys].[sp_set_session_context] @key = N'NeutralCulture', @value = @NeutralCulture;

    -- Get the User Id
    DECLARE 
        @UserId INT, 
        @Name NVARCHAR(255), 
        @Name2 NVARCHAR(255), 
        @Name3 NVARCHAR(255), 
        @ExternalId NVARCHAR(450), 
        @Email NVARCHAR(255), 
        @SettingsVersion UNIQUEIDENTIFIER, 
        @PermissionsVersion UNIQUEIDENTIFIER,
        @ViewsAndSpecsVersion UNIQUEIDENTIFIER,
        @UserSettingsVersion UNIQUEIDENTIFIER,
        @PrimaryLanguageId NVARCHAR(255),
        @PrimaryLanguageSymbol NVARCHAR(255),
        @SecondaryLanguageId NVARCHAR(255),
        @SecondaryLanguageSymbol NVARCHAR(255),
        @TernaryLanguageId NVARCHAR(255),
        @TernaryLanguageSymbol NVARCHAR(255);

    SELECT
        @UserId = [Id],
        @Name = [Name],
        @Name2 = [Name2],
        @Name3 = [Name3],
        @ExternalId = [ExternalId],
        @Email = [Email],
        @PermissionsVersion = [PermissionsVersion],
        @UserSettingsVersion = [UserSettingsVersion]
    FROM [dbo].[Users] 
    WHERE [IsActive] = 1 AND ([ExternalId] = @ExternalUserId OR [Email] = @UserEmail);

    -- Set LastAccess (Works only when @UserId IS NOT NULL)
    UPDATE [dbo].[Users] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Get hashes
    SELECT 
        @SettingsVersion = [SettingsVersion],
        @ViewsAndSpecsVersion = [ViewsAndSpecsVersion],
        @PrimaryLanguageId = [PrimaryLanguageId],
        @PrimaryLanguageSymbol = [PrimaryLanguageSymbol],
        @SecondaryLanguageId = [SecondaryLanguageId],
        @SecondaryLanguageSymbol = [SecondaryLanguageSymbol]
    FROM [dbo].[Settings]

    -- Set the User Id
    EXEC sp_set_session_context @key = N'UserId', @value = @UserId;

    -- Return the user and tenant information
    SELECT 
		-- User Info
        @UserId AS userId, 
        @Name AS Name,
        @Name2 AS Name2,
        @ExternalId AS ExternalId, 
        @Email AS Email, 
        @PermissionsVersion AS PermissionsVersion,
        @UserSettingsVersion AS UserSettingsVersion,
		-- Tenant Info
        @ViewsAndSpecsVersion AS ViewsAndSpecsVersion,
        @SettingsVersion AS SettingsVersion, 
        @PrimaryLanguageId AS PrimaryLanguageId,
        @PrimaryLanguageSymbol AS PrimaryLanguageSymbol,
        @SecondaryLanguageId AS SecondaryLanguageId,
        @SecondaryLanguageSymbol AS SecondaryLanguageSymbol;
