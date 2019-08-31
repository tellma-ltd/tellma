CREATE PROCEDURE [dal].[OnConnect]
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50),
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255)
AS
BEGIN
    -- Set the global values of the session context
    EXEC sp_set_session_context @key = N'Culture', @value = @Culture;
    EXEC sp_set_session_context @key = N'NeutralCulture', @value = @NeutralCulture;

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
        @ShortCompanyName NVARCHAR(255), 
        @ShortCompanyName2 NVARCHAR(255), 
        @ShortCompanyName3 NVARCHAR(255), 
		@PrimaryLanguageId NVARCHAR(255),
        @PrimaryLanguageSymbol NVARCHAR(255),
        @SecondaryLanguageId NVARCHAR(255),
        @SecondaryLanguageSymbol NVARCHAR(255),
        @TernaryLanguageId NVARCHAR(255),
        @TernaryLanguageSymbol NVARCHAR(255);

    SELECT
        @UserId				= U.[Id],
        @Name				= A.[Name],
        @Name2				= A.[Name2],
        @Name3				= A.[Name3],
        @ExternalId			= U.[ExternalId],
        @Email				= U.[Email],
        @PermissionsVersion = U.[PermissionsVersion],
        @UserSettingsVersion = U.[UserSettingsVersion]
    FROM [dbo].[Users] U
	JOIN [dbo].[Agents] A ON U.[Id] = A.[Id]
    WHERE A.[IsActive] = 1
	AND ([ExternalId] = @ExternalUserId OR [Email] = @UserEmail);

    -- Set LastAccess (Works only when @UserId IS NOT NULL)
    UPDATE [dbo].[Users] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Get hashes
    SELECT 
		@ShortCompanyName		= [ShortCompanyName],
		@ShortCompanyName2		= [ShortCompanyName2],
		@ShortCompanyName3		= [ShortCompanyName3],
		@SettingsVersion		= [SettingsVersion],
        @ViewsAndSpecsVersion	= [ViewsAndSpecsVersion],
        @PrimaryLanguageId		= [PrimaryLanguageId],
        @PrimaryLanguageSymbol	= [PrimaryLanguageSymbol],
        @SecondaryLanguageId	= [SecondaryLanguageId],
        @SecondaryLanguageSymbol= [SecondaryLanguageSymbol],
        @TernaryLanguageId		= [TernaryLanguageId],
        @TernaryLanguageSymbol	= [TernaryLanguageSymbol]
    FROM [dbo].[Settings]

    -- Set the User Id
    EXEC sp_set_session_context @key = N'UserId', @value = @UserId;

    -- Return the user information
    SELECT 
		-- User Info
        @UserId AS [UserId], 
        @Name AS [Name],
        @Name2 AS [Name2],
		@Name3 As [Name3],
        @ExternalId AS [ExternalId], 
        @Email AS [Email], 
        @PermissionsVersion AS [PermissionsVersion],
        @UserSettingsVersion AS [UserSettingsVersion],
		-- Tenant Info
		@ShortCompanyName AS [ShortCompanyName],
		@ShortCompanyName2 AS [ShortCompanyName2],
		@ShortCompanyName3 AS [ShortCompanyName3],
        @ViewsAndSpecsVersion AS [ViewsAndSpecsVersion],
        @SettingsVersion AS [SettingsVersion], 
        @PrimaryLanguageId AS [PrimaryLanguageId],
        @PrimaryLanguageSymbol AS [PrimaryLanguageSymbol],
        @SecondaryLanguageId AS [SecondaryLanguageId],
        @SecondaryLanguageSymbol AS [SecondaryLanguageSymbol],
        @TernaryLanguageId AS [TernaryLanguageId],
        @TernaryLanguageSymbol AS [TernaryLanguageSymbol];
END;