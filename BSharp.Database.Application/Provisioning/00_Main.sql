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

:r .\00_Setup\a_AdminSetup.sql
:r .\00_Setup\b_Settings.sql
:r .\00_Setup\c_ContractTypes.sql
--:r .\01_AccountsEntries\a_AccountGroups.sql
--:r .\01_AccountsEntries\b_AccountTypes.sql
--:r .\01_AccountsEntries\d_AccountTypesEntryTypes.sql
--:r .\01_AccountsEntries\x_AccountClassifications.sql
--:r .\03_DocumentsLines\a_LineDefinitions.sql
--:r .\03_DocumentsLines\b_LineDefinitionEntries.sql
--:r .\03_DocumentsLines\b_DocumentDefinitions.sql