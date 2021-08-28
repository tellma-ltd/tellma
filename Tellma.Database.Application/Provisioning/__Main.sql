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
	:r .\000\j_AgentDefinitions.sql
	:r .\000\l_ResourceDefinitions.sql
	:r .\000\m_AccountTypes.sql

	:r .\000\n_Settings.sql
	:r .\000\o_LineDefinitions.sql
	:r .\000\p_DocumentDefinitions.sql
	----:r .\000\q_ReportDefinitions.sql
	:r .\000\r_AccountClassifications.sql
	:r .\000\t_Accounts.sql
	:r .\000\y_Roles.sql
	:r .\000\z_Translations.sql

	UPDATE Settings SET DefinitionsVersion = NewId();
	RETURN;
	ERR_LABEL:
	RETURN;
END
