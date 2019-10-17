BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'support@banan-it.com';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @PrimaryLanguageId NVARCHAR(255)		= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
	DECLARE @ChartOfAccounts NVARCHAR(255)			= NULL; --'$(CHartOfAccounts)';
END
-- Local Variables
DECLARE @AdminUserId INT, @RoleId INT, @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET(), @ValidationErrorsJson NVARCHAR(MAX);

:r .\00_Setup\a_AdminSetup.sql
:r .\00_Setup\b_Settings.sql
:r .\01_AccountsEntries\a_AccountDefinitions.sql
:r .\01_AccountsEntries\b_AccountTypes.sql
:r .\01_AccountsEntries\c_EntryTypes.sql
:r .\01_AccountsEntries\d_AccountTypesEntryTypes.sql
:r .\01_AccountsEntries\x_AccountClassifications.sql
:r .\02_ResourcesUnits\a_Currencies.sql
:r .\02_ResourcesUnits\b_MeasurementUnits.sql
:r .\02_ResourcesUnits\c_ResourceTypes.sql

--:r .\02_Accounts.sql
--EXEC [dbo].[adm_Accounts_Notes__Update];
--:r .\04_AccountsNotes.sql
:r .\06_DocumentDefinitions.sql
--:r .\05_LineTypeSpecifications.sql
--:r .\07_AccountSpecifications.sql

--:r .\90_IfrsConcepts.sql
--:r .\91_IfrsDisclosures.sql
