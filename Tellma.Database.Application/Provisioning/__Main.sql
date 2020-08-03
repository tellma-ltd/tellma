:r .\000\a_Declarations.sql
:r .\000\b_AdminUser.sql
IF @OverwriteDb = 0 RETURN

:r .\000\c_Currencies.sql
:r .\000\d_Units.sql
:r .\000\e_RuleTypes.sql
--:r .\000\f_IfrsConcepts.sql
--:r .\000\g_IfrsDisclosures.sql
:r .\000\h_EntryTypes.sql
:r .\000\i_LookupDefinitions.sql
:r .\000\j_RelationDefinitions.sql
:r .\000\k_CustodyDefinitions.sql
:r .\000\l_ResourceDefinitions.sql
:r .\000\m_AccountTypes.sql

:r .\000\n_Settings.sql
:r .\000\o_LineDefinitions.sql

:r .\000\p_DocumentDefinitions.sql
:r .\000\q_ReportDefinitions.sql
:r .\000\r_AccountClassifications.sql
:r .\000\t_Accounts.sql
:r .\000\u_Lookups.sql
:r .\000\y_Roles.sql
:r .\000\z_Translations.sql

IF @DB IN (N'100', N'103', N'104', N'105')
BEGIN
	:r .\100_105\01_Security\a_Users.sql
	:r .\100_105\01_Security\b_Permissions.sql

	:r .\100_105\02_Basic\b_Centers.sql

	:r .\100_105\04_Resources\104_finished_goods.sql
	:r .\100_105\04_Resources\104_raw-materials.sql
	:r .\100_105\04_Resources\105_merchandise.sql
	:r .\100_105\05_Contracts\01_CashCustodies.sql
	:r .\100_105\05_Contracts\02_InventoryCustodies.sql
	:r .\100_105\05_Contracts\03_Customers.sql
	:r .\100_105\05_Contracts\05_Partners.sql
	:r .\100_105\05_Contracts\06_Suppliers.sql
	:r .\100_105\05_Contracts\08_Employees.sql
END
IF @DB = N'101' -- Banan SD, en/ar
BEGIN
	:r .\101\101_01Currencies.sql
	:r .\101\101_02EntryTypes.sql
	:r .\101\101_03LookupDefinitions.sql
	:r .\101\101_04ResourceDefinitions.sql
	:r .\101\101_05CustodyDefinitions.sql
	:r .\101\101_06RelationDefinitions.sql
	:r .\101\101_07DocumentDefinitions.sql
	:r .\101\101_11Users.sql
	:r .\101\101_12Permissions.sql
	:r .\101\101_13Workflows.sql
	:r .\101\101_21Lookups.sql
	:r .\101\101_22Centers.sql
	:r .\101\101_23Resources.sql
	:r .\101\101_24Relations.sql
	:r .\101\101_25Custodies.sql
	:r .\101\101_31Accounts.sql
END
IF @DB = N'102' -- Banan SD, en/ar
BEGIN
	:r .\102\102_01Currencies.sql
	:r .\102\102_02EntryTypes.sql
	:r .\102\102_03LookupDefinitions.sql
	:r .\102\102_04ResourceDefinitions.sql
	:r .\102\102_05CustodyDefinitions.sql
	:r .\102\102_06RelationDefinitions.sql
	:r .\102\102_07DocumentDefinitions.sql
	:r .\102\102_11Users.sql
	:r .\102\102_12Permissions.sql
	:r .\102\102_13Workflows.sql
	:r .\102\102_21Lookups.sql
	:r .\102\102_22Centers.sql
	:r .\102\102_23Resources.sql
	:r .\102\102_24Relations.sql
	:r .\102\102_25Custodies.sql
	:r .\102\102_31Accounts.sql
END
IF @DB = N'106' -- Soreti, ETB, en/am
BEGIN
	:r .\106\106_01Currencies.sql
	:r .\106\106_02EntryTypes.sql
	:r .\106\106_03LookupDefinitions.sql
	:r .\106\106_04ResourceDefinitions.sql
	:r .\106\106_05CustodyDefinitions.sql
	:r .\106\106_06RelationDefinitions.sql
	:r .\106\106_07DocumentDefinitions.sql
	:r .\106\106_11Users.sql
	:r .\106\106_12Permissions.sql
	:r .\106\106_13Workflows.sql
	:r .\106\106_21Lookups.sql
	:r .\106\106_22Centers.sql
	:r .\106\106_24Relations.sql
	:r .\106\106_25Custodies.sql
	:r .\106\106_31Accounts.sql
END
IF @DB = N'107' -- SSIA, SDG, en/ar
BEGIN

	:r .\107\00_Script.sql

	:r .\107\10_Users.sql
	:r .\107\11_Permissions.sql

	:r .\107\107_21Lookups.sql
	:r .\107\21_Units.sql
	:r .\107\107_22Centers.sql

	:r .\107\30_Resources.sql
	
	:r .\107\40_Agents.sql
	:r .\107\50_Contracts.sql

	:r .\107\60_AccountClassifications.sql
	:r .\107\61_Accounts.sql
END
IF @DB = N'108' -- SSIA - HG, SDG, en/ar
BEGIN
	:r .\108\00_Setup\Script.sql
	--:r .\108\01_Security\a_Users.sql
	--:r .\108\01_Security\b_Permissions.sql

	--:r .\108\02_Basic\a_Lookups.sql
	--:r .\108\02_Basic\b_Centers.sql
	
	--:r .\108\05_Contracts\00_Agents.sql
	--:r .\108\05_Contracts\01_CashCustodies.sql
	--:r .\108\05_Contracts\02_InventoryCustodies.sql
	--:r .\108\05_Contracts\03_Customers.sql
	--:r .\108\05_Contracts\04_Debtors.sql
	--:r .\108\05_Contracts\05_Partners.sql
	--:r .\108\05_Contracts\06_Suppliers.sql
	--:r .\108\05_Contracts\07_Creditors.sql
	--:r .\108\05_Contracts\08_Employees.sql
	--:r .\108\06_Accounts\b_Accounts.sql
END

--UPDATE Settings SET DefinitionsVersion = NewId()
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