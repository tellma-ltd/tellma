:r .\000\a_Declarations.sql
:r .\000\b_AdminUser.sql
IF @OverwriteDb = 0 RETURN



:r .\000\d_RuleTypes.sql
:r .\000\e_EntryTypes.sql

:r .\000\f_LookupDefinitions.sql
:r .\000\g_ResourceDefinitions.sql
:r .\000\h_ContractDefinitions.sql
:r .\000\i_AccountTypes.sql
--:r .\000\j_IfrsConcepts.sql
--:r .\000\k_IfrsDisclosures.sql
:r .\000\l_Currencies.sql
:r .\000\m_Units.sql
:r .\000\n_Settings.sql
:r .\000\o_LineDefinitions.sql

:r .\000\p_DocumentDefinitions.sql
:r .\000\q_ReportDefinitions.sql

:r .\000\x_SampleContracts.sql
:r .\000\y_Roles.sql
:r .\000\z_Translations.sql

IF @DB IN (N'100', N'101', N'102', N'103', N'104', N'105')
BEGIN
	:r .\100_105\00_Setup\Script.sql
	:r .\100_105\01_Security\a_Users.sql
	:r .\100_105\01_Security\b_Permissions.sql

	:r .\100_105\02_Basic\a_Lookups.sql
	:r .\100_105\02_Basic\b_Centers.sql
	:r .\100_105\02_Basic\c_Units.sql

	:r .\100_105\04_Resources\101_property-plant-and-equipment.sql
	:r .\100_105\04_Resources\101_employee-benefits.sql
	:r .\100_105\04_Resources\101_revenue_services.sql
	:r .\100_105\04_Resources\102_employee-benefits.sql
	:r .\100_105\04_Resources\102_property-plant-and-equipment.sql
	:r .\100_105\04_Resources\104_finished_goods.sql
	:r .\100_105\04_Resources\104_raw-materials.sql
	:r .\100_105\04_Resources\105_merchandise.sql

	:r .\100_105\04_Resources\a1_PPE_motor-vehicles.sql
	:r .\100_105\04_Resources\a3_PPE_machineries.sql

	:r .\100_105\04_Resources\d1_FG_vehicles.sql
	--:r .\100_105\04_Resources\e1_CCE_received-checks.sql


	:r .\100_105\05_Contracts\00_Agents.sql
	:r .\100_105\05_Contracts\01_CashCustodians.sql
	:r .\100_105\05_Contracts\02_InventoryCustodians.sql
	:r .\100_105\05_Contracts\03_Customers.sql
	:r .\100_105\05_Contracts\04_Debtors.sql
	:r .\100_105\05_Contracts\05_Partners.sql
	:r .\100_105\05_Contracts\06_Suppliers.sql
	:r .\100_105\05_Contracts\07_Creditors.sql
	:r .\100_105\05_Contracts\08_Employees.sql

	:r .\100_105\06_Accounts\a_AccountClassifications.sql
	:r .\100_105\06_Accounts\b_Accounts.sql

	--IF @DB = N'101'
	--BEGIN
	--:r .\100_105\07_Entries\101\a_manual-journal-vouchers.sql
	--:r .\100_105\07_Entries\101\b_cash-payment-vouchers.sql
	--:r .\100_105\07_Entries\101\e_revenue-templates.sql
	--:r .\100_105\07_Entries\101\f_revenue-recognition-vouchers.sql
	--END
END
IF @DB = N'106' -- Soreti, ETB, en/am
BEGIN
	:r .\106\00_Setup\Script.sql
	:r .\106\01_Security\a_Users.sql
	:r .\106\01_Security\b_Permissions.sql

	:r .\106\02_Basic\a_Lookups.sql
	:r .\106\02_Basic\b_Centers.sql
	:r .\106\02_Basic\c_Units.sql
	
	:r .\106\05_Contracts\00_Agents.sql
	:r .\106\05_Contracts\01_CashCustodians.sql
	:r .\106\05_Contracts\02_InventoryCustodians.sql
	:r .\106\05_Contracts\03_Customers.sql
	:r .\106\05_Contracts\04_Debtors.sql
	:r .\106\05_Contracts\05_Partners.sql
	:r .\106\05_Contracts\06_Suppliers.sql
	:r .\106\05_Contracts\07_Creditors.sql
	:r .\106\05_Contracts\08_Employees.sql

	:r .\106\06_Accounts\a_AccountClassifications.sql
	:r .\106\06_Accounts\b_Accounts.sql
END
IF @DB = N'107' -- SSIA, SDG, en/ar
BEGIN
	:r .\000\r_AccountClassifications.sql
	:r .\107\00_Setup\Script.sql
	:r .\107\01_Security\a_Users.sql
	:r .\107\01_Security\b_Permissions.sql

	:r .\107\02_Basic\a_Lookups.sql
	:r .\107\02_Basic\b_Centers.sql
	:r .\107\02_Basic\c_Units.sql
	
	:r .\107\05_Contracts\00_Agents.sql
	:r .\107\05_Contracts\01_CashCustodians.sql
	:r .\107\05_Contracts\02_InventoryCustodians.sql
	:r .\107\05_Contracts\03_Customers.sql
	:r .\107\05_Contracts\04_Debtors.sql
	:r .\107\05_Contracts\05_Partners.sql
	:r .\107\05_Contracts\06_Suppliers.sql
	:r .\107\05_Contracts\07_Creditors.sql
	:r .\107\05_Contracts\08_Employees.sql

	--:r .\107\06_Accounts\a_AccountClassifications.sql
	:r .\107\06_Accounts\b_Accounts.sql
END
IF @DB = N'108' -- SSIA - HG, SDG, en/ar
BEGIN
	:r .\108\00_Setup\Script.sql
	--:r .\108\01_Security\a_Users.sql
	--:r .\108\01_Security\b_Permissions.sql

	--:r .\108\02_Basic\a_Lookups.sql
	--:r .\108\02_Basic\b_Centers.sql
	--:r .\108\02_Basic\c_Units.sql
	
	--:r .\108\05_Contracts\00_Agents.sql
	--:r .\108\05_Contracts\01_CashCustodians.sql
	--:r .\108\05_Contracts\02_InventoryCustodians.sql
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