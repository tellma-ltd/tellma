:r .\000\a_Declarations.sql
--IF @OverwriteDb = 0 RETURN
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

--IF @DB IN (N'103', N'104', N'105')
--BEGIN
--	:r .\103_105\01_Security\a_Users.sql
--	:r .\103_105\01_Security\b_Permissions.sql

--	:r .\103_105\02_Basic\b_Centers.sql

--	:r .\103_105\04_Resources\104_finished_goods.sql
--	:r .\103_105\04_Resources\104_raw-materials.sql
--	:r .\103_105\04_Resources\105_merchandise.sql
--	:r .\103_105\05_Contracts\01_CashCustodies.sql
--	:r .\103_105\05_Contracts\02_InventoryCustodies.sql
--	:r .\103_105\05_Contracts\03_Customers.sql
--	:r .\103_105\05_Contracts\05_Partners.sql
--	:r .\103_105\05_Contracts\06_Suppliers.sql
--	:r .\103_105\05_Contracts\08_Employees.sql
--END
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
--	--:r .\100\100_13Workflows.sql
--	:r .\100\100_21Lookups.sql
--	:r .\100\100_22Centers.sql
--	:r .\100\100_23Relations.sql
--	:r .\100\100_24Resources.sql
--	:r .\100\100_25Custodies.sql
--	:r .\100\100_31Accounts.sql
--END
--IF @DB = N'200' -- Banan ET, en
--BEGIN
--	:r .\200\200_01Currencies.sql
--	:r .\200\200_02EntryTypes.sql
--	:r .\200\200_03LookupDefinitions.sql
--	:r .\200\200_04ResourceDefinitions.sql
--	:r .\200\200_06RelationDefinitions.sql
--	--:r .\200\200_07DocumentDefinitions.sql
--	:r .\200\200_11Users.sql
--	:r .\200\200_12Permissions.sql
--	--:r .\200\200_13Workflows.sql
--	:r .\200\200_21Lookups.sql
--	:r .\200\200_22Centers.sql
--	:r .\200\200_23Relations.sql
--	:r .\200\200_24Resources.sql
--	:r .\200\200_25Custodies.sql
--	:r .\200\200_31Accounts.sql
--END
--IF @DB = N'201' -- Soreti, ETB, en/am
--BEGIN
--	:r .\201\201_01Currencies.sql
--	:r .\201\201_02EntryTypes.sql
--	:r .\201\201_03LookupDefinitions.sql
--	:r .\201\201_04ResourceDefinitions.sql
--	:r .\201\201_06RelationDefinitions.sql
--	--:r .\201\201_07DocumentDefinitions.sql
--	:r .\201\201_11Users.sql
--	:r .\201\201_12Permissions.sql
--	--:r .\201\201_13Workflows.sql
--	:r .\201\201_21Lookups.sql
--	:r .\201\201_22Centers.sql
--	:r .\201\201_23Relations.sql
--	:r .\201\201_24Resources.sql
--	:r .\201\201_25Custodies.sql
--	:r .\201\201_31Accounts.sql
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
	
--	:r .\199\40_Agents.sql
----	:r .\199\50_Contracts.sql

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