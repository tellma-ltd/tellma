BEGIN -- Setup Configuration
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'admin@tellma.com';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)'; --N'ACME International';
	DECLARE @ShortCompanyName2 NVARCHAR(255);
	DECLARE @ShortCompanyName3 NVARCHAR(255);
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

	DECLARE @Resources dbo.ResourceList, @ResourceUnits dbo.ResourceUnitList;
END

:r .\00_Common\__Declarations.sql
:r .\00_Common\a_AdminUser.sql
:r .\00_Common\b_FunctionalCurrency.sql
:r .\00_Common\c_Settings.sql
:r .\00_Common\d_EntryTypes.sql
:r .\00_Common\e_AccountTypes.sql

:r .\01_Definitions\a_LookupDefinitions.sql
:r .\01_Definitions\b_ResourceDefinitions.sql
:r .\01_Definitions\c_AgentDefinitions.sql

:r .\01_Definitions\e_LineDefinitions\000.sql
:r .\01_Definitions\e_LineDefinitions\100.sql
:r .\01_Definitions\e_LineDefinitions\101.sql
:r .\01_Definitions\e_LineDefinitions\102.sql
:r .\01_Definitions\e_LineDefinitions\103.sql
:r .\01_Definitions\e_LineDefinitions\104.sql
:r .\01_Definitions\e_LineDefinitions\105.sql
:r .\01_Definitions\e_LineDefinitions\999.sql

:r .\01_Definitions\f_DocumentDefinitions.sql

:r .\02_Security\a_Users.sql
:r .\02_Security\b_RolesMemberships.sql
:r .\02_Security\c_Workflows.sql

:r .\03_Basic\a_Currencies.sql
:r .\03_Basic\b_MeasurementUnits.sql
:r .\03_Basic\c_Lookups.sql
		
:r .\05_Resources\a1_PPE_motor-vehicles.sql
:r .\05_Resources\a2_PPE_computer-equipment.sql
:r .\05_Resources\a3_PPE_machineries.sql
:r .\05_Resources\a4_PPE_general-fixed-assets.sql
:r .\05_Resources\b_Inventories_raw-materials.sql
:r .\05_Resources\d1_FG_vehicles.sql
:r .\05_Resources\d2_FG_steel-products.sql
:r .\05_Resources\d3_TM_paper-products.sql
--:r .\05_Resources\e1_CCE_received-checks.sql
:r .\05_Resources\h_PL_employee-benefits.sql

:r .\04_Agents\00_ResponsibilityCenters.sql
:r .\04_Agents\01_CostObjects.sql
:r .\04_Agents\02_Creditors.sql
:r .\04_Agents\03_Customers.sql
:r .\04_Agents\04_Debtors.sql
--:r .\04_Agents\05_Owners.sql
:r .\04_Agents\06_Suppliers.sql
--:r .\04_Agents\07_TaxAgencies.sql
:r .\04_Agents\08_Banks.sql
:r .\04_Agents\09_Custodies.sql
:r .\04_Agents\09a_Warehouses.sql
:r .\04_Agents\10_Employees.sql

:r .\06_Accounts\a_LegacyTypes.sql
:r .\06_Accounts\b_LegacyClassifications.sql
:r .\06_Accounts\c_BasicAccounts.sql
:r .\06_Accounts\d_SmartAccounts.sql
:r .\06_Accounts\000.sql
:r .\06_Accounts\101.sql
:r .\06_Accounts\105.sql
:r .\06_Accounts\999.sql

:r .\07_Entries\01_manual-journal-vouchers.sql
:r .\07_Entries\01E5_manual-journal-vouchers.sql
:r .\07_Entries\02_cash-payment-vouchers.sql

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

IF @DB = N'100' -- ACME, USD, en/ar/zh playground
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
BEGIN
	Print N'Tellma'
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
BEGIN
	Print N'Tellma'
END