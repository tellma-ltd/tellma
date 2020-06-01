:r .\000\a_Declarations.sql
:r .\000\b_AdminUserRole.sql
:r .\000\c_RuleTypes.sql
:r .\000\d_EntryTypes.sql
:r .\000\e_AccountTypes.sql
:r .\000\f_IfrsConcepts.sql
--:r .\000\g_IfrsDisclosures.sql
:r .\000\h_FunctionalCurrency.sql
:r .\000\i_Settings.sql
RETURN

IF (1=1)-- @DB <> N'106' -- Banan SD, USD, en
BEGIN
	:r .\01_Security\a_Users.sql
	:r .\01_Security\b_RolesMemberships.sql

	:r .\02_Definitions\a_LookupDefinitions.sql
	:r .\02_Definitions\b_ResourceDefinitions.sql
	:r .\02_Definitions\c_ContractDefinitions.sql
	:r .\02_Definitions\f_LineDefinitions.sql
	:r .\02_Definitions\g_DocumentDefinitions.sql

	:r .\03_Basic\a_Currencies.sql
	:r .\03_Basic\b_Units.sql
	:r .\03_Basic\c_Lookups.sql
	:r .\03_Basic\d_Segments.sql
	:r .\03_Basic\e_Centers.sql
END
IF @DB <> N'106'
BEGIN
	:r .\04_Resources\101_property-plant-and-equipment.sql
	:r .\04_Resources\101_employee-benefits.sql
	:r .\04_Resources\101_revenue_services.sql
	--:r .\04_Resources\102_employee-benefits.sql
	--:r .\04_Resources\102_property-plant-and-equipment.sql
	--:r .\04_Resources\104_finished_goods.sql
	--:r .\04_Resources\104_raw-materials.sql
	--:r .\04_Resources\105_merchandise.sql

	--:r .\04_Resources\a1_PPE_motor-vehicles.sql
	--:r .\04_Resources\a3_PPE_machineries.sql

	--:r .\04_Resources\d1_FG_vehicles.sql
	----:r .\04_Resources\e1_CCE_received-checks.sql
	:r .\05_Contracts\00_Agents.sql
	:r .\05_Contracts\01_CashCustodians.sql
	:r .\05_Contracts\02_InventoryCustodians.sql
	:r .\05_Contracts\03_Customers.sql
	:r .\05_Contracts\04_Debtors.sql
	:r .\05_Contracts\05_Partners.sql
	:r .\05_Contracts\06_Suppliers.sql
	:r .\05_Contracts\07_Creditors.sql
	:r .\05_Contracts\08_Employees.sql

	:r .\06_Accounts\101_AccountClassifications.sql
--	:r .\06_Accounts\101_Accounts.sql
	--:r .\07_Entries\101\101a_manual-journal-vouchers.sql
	--:r .\07_Entries\101\101b_cash-payment-vouchers.sql
	--:r .\07_Entries\101\101e_revenue-templates.sql
	--:r .\07_Entries\101\101f_revenue-recognition-vouchers.sql
END

IF @DB = N'106' -- Soreti, ETB, en/am
BEGIN
	:r .\106\05_Contracts\00_Agents.sql
	:r .\106\05_Contracts\01_CashCustodians.sql
	:r .\106\05_Contracts\02_InventoryCustodians.sql
	:r .\106\05_Contracts\03_Customers.sql
	:r .\106\05_Contracts\04_Debtors.sql
	:r .\106\05_Contracts\05_Partners.sql
	:r .\106\05_Contracts\06_Suppliers.sql
	:r .\106\05_Contracts\07_Creditors.sql
	:r .\106\05_Contracts\08_Employees.sql

	:r .\106\06_Accounts\a_AccountTypeContractDefinitions.sql
	:r .\106\06_Accounts\b_AccountClassifications.sql
	:r .\106\06_Accounts\c_Accounts.sql
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