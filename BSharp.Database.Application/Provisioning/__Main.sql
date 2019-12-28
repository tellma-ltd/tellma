BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'admin@bsharp.online';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @PrimaryLanguageId NVARCHAR(255)		= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @SecondaryLanguageId NVARCHAR(255)		= '$(SecondaryLanguageId)'; --N'en';
	DECLARE @TernaryLanguageId NVARCHAR(255)		= '$(TernaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
	DECLARE @ChartOfAccounts NVARCHAR(255)			= NULL; --'$(ChartOfAccounts)';
	-- Because there is no way to pass the NULL value to 
	IF @SecondaryLanguageId = N'NULL' SET @SecondaryLanguageId = NULL;
	IF @TernaryLanguageId = N'NULL' SET @TernaryLanguageId = NULL;
END


-- Local Variables

-- Minimum required for provisioning:
-- Admin user, the same who started the instance, as well as 
-- resource definitions: N'currencies' & N'general-items'
-- Resource classifications: The Ifrs ones
-- functional currency
-- contract type
-- agent definitions: (local) customers, (local) suppliers, (local) employees, shareholders, banks, ...
-- all line definitions, some standard document definitions
:r .\00_Common\__Declarations.sql
:r .\00_Common\a_AdminUser.sql
:r .\00_Common\b_ResourceDefinitions.sql
:r .\00_Common\c_ResourceClassifications.sql
:r .\00_Common\d_EntryClassifications.sql
:r .\00_Common\e_ResourceClassificationsEntryClassifications.sql
:r .\00_Common\f_FunctionalCurrency.sql
:r .\00_Common\g_ContractTypes.sql
:r .\00_Common\h_AgentDefinitions.sql
:r .\00_Common\i_Settings.sql
:r .\00_Common\j_AccountTypes.sql
:r .\01_Security\a_Users.sql
:r .\01_Security\b_RolesMemberships.sql

RETURN;
ERR_LABEL:
	SELECT * FROM OpenJson(@ValidationErrorsJson)
	WITH (
		[Key] NVARCHAR (255) '$.Key',
		[ErrorName] NVARCHAR (255) '$.ErrorName',
		[Argument0] NVARCHAR (255) '$.Argument0',
		[Argument1] NVARCHAR (255) '$.Argument1',
		[Argument2] NVARCHAR (255) '$.Argument2',
		[Argument3] NVARCHAR (255) '$.Argument3',
		[Argument4] NVARCHAR (255) '$.Argument4'
	);
RETURN;