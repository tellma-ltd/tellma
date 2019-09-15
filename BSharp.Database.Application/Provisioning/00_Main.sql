BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)					= '$(DeployEmail)';-- N'support@banan-it.com';
	DECLARE @ShortCompanyName NVARCHAR(255)				= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @PrimaryLanguageId NVARCHAR(255)			= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrency NCHAR(3)				= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER		= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER			= NEWID();
END
-- Local Variables
DECLARE @UserId INT, @RoleId INT, @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET(), @FunctionalCurrencyId INT, @ValidationErrorsJson NVARCHAR(MAX);
-- Add the support account
IF NOT EXISTS(SELECT * FROM [dbo].[Users] WHERE [Email] = @DeployEmail)
BEGIN
	INSERT INTO dbo.Agents([Name],[AgentType], CreatedById, ModifiedById)
	VALUES (N'Banan IT', N'Organization', IDENT_CURRENT('dbo.Agents'), IDENT_CURRENT('dbo.Agents'));

	SET @UserId= SCOPE_IDENTITY();
	INSERT INTO [dbo].[Users]([Id], [Email], CreatedById, ModifiedById)
	VALUES (@UserId, @DeployEmail, @UserId, @UserId);
	
	INSERT INTO [dbo].[Roles] ([Name], [Name2], [Code], [IsPublic], [SavedById])
	VALUES (N'Administrator', N'المشرف', 'All', 0, @UserId)
	SET @RoleId= SCOPE_IDENTITY();

	INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action],  [SavedById])
	VALUES (@RoleId, N'all', N'All', @UserId)

	INSERT INTO [dbo].[RoleMemberships] ([AgentId], [RoleId], [SavedById])
	VALUES									(@UserId, @RoleId, @UserId)
END
-- Set the user session context
SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = @DeployEmail;
EXEC master.sys.sp_set_session_context 'UserId', @UserId;
--
EXEC [dal].[Settings__Save]
	@ShortCompanyName = @ShortCompanyName,
	@PrimaryLanguageId = @PrimaryLanguageId,
	@DefinitionsVersion = @DefinitionsVersion,
	@SettingsVersion = @SettingsVersion,
	@FunctionalCurrency = @FunctionalCurrency;

:r .\01_IfrsConcepts.sql
:r .\011_IfrsDisclosures.sql
:r .\012_IfrsEntryClassifications.sql
:r .\013_IfrsAccountClassifications.sql
:r .\02_MeasurementUnits.sql
--:r .\02_Accounts.sql
--EXEC [dbo].[adm_Accounts_Notes__Update];
--:r .\04_AccountsNotes.sql
:r .\06_DocumentDefinitions.sql
--:r .\05_LineTypeSpecifications.sql
--:r .\07_AccountSpecifications.sql