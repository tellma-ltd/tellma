BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'admin@bsharp.online';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @PrimaryLanguageId NVARCHAR(255)		= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
	DECLARE @ChartOfAccounts NVARCHAR(255)			= NULL; --'$(ChartOfAccounts)';
END
-- Local Variables
DECLARE @AdminUserId INT, @RoleId INT, @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET(), @ValidationErrorsJson NVARCHAR(MAX);

-- Minimum required for provisioning:
-- Admin user, the same who started the instance, as well as 
-- resource definitions: N'currencies' & N'general-items'
-- Resource classifications: The Ifrs ones
-- functional currency
-- contract type
-- agent definitions: (local) customers, (local) suppliers, (local) employees, shareholders, banks, ...
-- all line definitions, some standard document definitions
:r .\a_AdminUser.sql
:r .\b_ResourceDefinitions.sql
:r .\c_ResourceClassifications.sql
:r .\d_EntryClassifications.sql
:r .\e_ResourceClassificationsEntryClassifications.sql
:r .\f_FunctionalCurrency.sql
:r .\g_ContractTypes.sql
:r .\h_AgentDefinitions.sql
:r .\i_Settings.sql
:r .\j_AccountTypes.sql

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