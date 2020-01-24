CREATE PROCEDURE [dal].[OnConnect]
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50),
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255),
	@SetLastActive BIT = 1
AS
BEGIN
    -- Set the global values of the session context
	DECLARE @UserLanguageIndex TINYINT = [dbo].[fn_User__Language](@Culture, @NeutralCulture);
    EXEC master.sys.sp_set_session_context @key = N'UserLanguageIndex', @value = @UserLanguageIndex;

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
        @DefinitionsVersion UNIQUEIDENTIFIER,
        @UserSettingsVersion UNIQUEIDENTIFIER,
        @ShortCompanyName NVARCHAR(255), 
        @ShortCompanyName2 NVARCHAR(255), 
        @ShortCompanyName3 NVARCHAR(255), 
		@PrimaryLanguageId NVARCHAR(255),
        @PrimaryLanguageSymbol NVARCHAR(255),
        @SecondaryLanguageId NVARCHAR(255),
        @SecondaryLanguageSymbol NVARCHAR(255),
        @TernaryLanguageId NVARCHAR(255),
        @TernaryLanguageSymbol NVARCHAR(255),
		@FunctionalCurrencyId NCHAR(3);

    SELECT
        @UserId				= [Id],
        @Name				= [Name],
        @Name2				= [Name2],
        @Name3				= [Name3],
        @ExternalId			= [ExternalId],
        @Email				= [Email],
        @PermissionsVersion = [PermissionsVersion],
        @UserSettingsVersion = [UserSettingsVersion]
    FROM [dbo].[Users]
    WHERE [IsActive] = 1
	AND ([ExternalId] = @ExternalUserId OR [Email] = @UserEmail);

    -- Set LastAccess (Works only when @UserId IS NOT NULL)
    IF (@SetLastActive = 1)
        UPDATE [dbo].[Users] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Get settings
    SELECT 
		@ShortCompanyName		= [ShortCompanyName],
		@ShortCompanyName2		= [ShortCompanyName2],
		@ShortCompanyName3		= [ShortCompanyName3],
		@SettingsVersion		= [SettingsVersion],
        @DefinitionsVersion		= [DefinitionsVersion],
        @PrimaryLanguageId		= [PrimaryLanguageId],
        @PrimaryLanguageSymbol	= [PrimaryLanguageSymbol],
        @SecondaryLanguageId	= [SecondaryLanguageId],
        @SecondaryLanguageSymbol= [SecondaryLanguageSymbol],
        @TernaryLanguageId		= [TernaryLanguageId],
        @TernaryLanguageSymbol	= [TernaryLanguageSymbol],
		@FunctionalCurrencyId   = [FunctionalCurrencyId]
    FROM [dbo].[Settings]

    -- Set the User Id
    EXEC master.sys.sp_set_session_context @key = N'UserId', @value = @UserId;

	-- Set the Functional Currency Id
    EXEC master.sys.sp_set_session_context @key = N'FunctionalCurrencyId', @value = @FunctionalCurrencyId;

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
        @DefinitionsVersion AS [DefinitionsVersion],
        @SettingsVersion AS [SettingsVersion], 
        @PrimaryLanguageId AS [PrimaryLanguageId],
        @PrimaryLanguageSymbol AS [PrimaryLanguageSymbol],
        @SecondaryLanguageId AS [SecondaryLanguageId],
        @SecondaryLanguageSymbol AS [SecondaryLanguageSymbol],
        @TernaryLanguageId AS [TernaryLanguageId],
        @TernaryLanguageSymbol AS [TernaryLanguageSymbol];
END;