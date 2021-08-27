--IF DB_NAME() = N'Tellma.Tests.101'
--BEGIN
--	-- Provision data for unit tests
--	:r .\Tests\00_Declarations.sql
--	:r .\Tests\01_Users.sql
--	:r .\Tests\02_Roles.sql
--END
--GO -- Important so that variable declarations do not conflict

IF DB_NAME() <> N'Tellma.Tests.101'
BEGIN
	:r .\000\a_Declarations.sql
	IF @OverwriteDb = 0 RETURN
	:r .\000\b_AdminUser.sql
	:r .\000\c_Currencies.sql
	:r .\000\d_Units.sql
	:r .\000\e_RuleTypes.sql
	--:r .\000\f_IfrsConcepts.sql
	--:r .\000\g_IfrsDisclosures.sql
	:r .\000\h_EntryTypes.sql
	:r .\000\i_LookupDefinitions.sql
	:r .\000\j_RelationDefinitions.sql
	:r .\000\l_ResourceDefinitions.sql
	:r .\000\m_AccountTypes.sql

	:r .\000\n_Settings.sql
	:r .\000\o_LineDefinitions.sql
	:r .\000\p_DocumentDefinitions.sql
	----:r .\000\q_ReportDefinitions.sql
	:r .\000\r_AccountClassifications.sql
	:r .\000\t_Accounts.sql
	--:r .\000\u_Lookups.sql
	:r .\000\y_Roles.sql
	:r .\000\z_Translations.sql
	--IF @DB = N'999'
	--	UPDATE Currencies SET IsActive = 1 WHERE [Id] IN (N'ETB', N'USD');

	--IF @DB = N'100' -- Banan SD, en/ar
	--BEGIN
	--	:r .\100\100_01Currencies.sql
	--	:r .\100\100_02EntryTypes.sql
	--	:r .\100\100_03LookupDefinitions.sql
	--	:r .\100\100_04ResourceDefinitions.sql
	--	:r .\100\100_06RelationDefinitions.sql
	--	--:r .\100\100_07DocumentDefinitions.sql
	--	:r .\100\100_11Users.sql
	--	:r .\100\100_12Permissions.sql
	--	:r .\100\100_21Lookups.sql
	--	:r .\100\100_22Centers.sql
	--	:r .\100\100_23Relations.sql
	--	:r .\100\100_24Resources.sql
	--	:r .\100\100_25Custodies.sql
	--	:r .\100\100_31Accounts.sql
	--END

	--IF @DB = N'199' -- SSIA, SDG, en/ar
	--BEGIN

	--	:r .\199\00_Script.sql

	--	:r .\199\199_11Users.sql
	--	:r .\199\199_12Permissions.sql

	--	:r .\199\199_21Lookups.sql
	--	:r .\199\21_Units.sql
	--	:r .\199\199_22Centers.sql

	--	:r .\199\199_24Resources.sql
	

	--	--:r .\199\60_AccountClassifications.sql
	--	--:r .\199\61_Accounts.sql
	--END
	--IF @DB = N'299' -- ET Demos, ETB, en/am/om
	--BEGIN

	--	:r .\299\00_Script.sql

	--	:r .\299\299_11Users.sql
	--	:r .\299\299_12Permissions.sql

	--	:r .\299\299_21Lookups.sql
	--	:r .\299\21_Units.sql
	--	:r .\299\299_22Centers.sql

	--	:r .\299\299_24Resources.sql
	--END

	UPDATE Settings SET DefinitionsVersion = NewId();
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
END
