		:r .\00_Setup\a_Declarations.sql
		:r .\00_Setup\b_RolesMemberships.sql
		:r .\00_Setup\z_LookupDefinitions.sql

		:r .\01_Basic\a_Currencies.sql
		:r .\01_Basic\b_MeasurementUnits.sql
		:r .\01_Basic\c_Lookups.sql
		
		:r .\02_Agents\01_ResponsibilityCenters.sql
		:r .\02_Agents\02_Suppliers.sql
		:r .\02_Agents\03_Customers.sql
		:r .\02_Agents\04_Employees.sql
		:r .\02_Agents\05_Banks.sql
		:r .\02_Agents\06_Custodies.sql

		:r .\03_Resources\a1_PPE_motor-vehicles.sql
		:r .\03_Resources\a2_PPE_it-equipment.sql
		:r .\03_Resources\a3_PPE_machineries.sql
		:r .\03_Resources\a4_PPE_general-fixed-assets.sql
		:r .\03_Resources\b_Inventories_raw-materials.sql
		:r .\03_Resources\d1_FG_vehicles.sql

		--:r .\03_Resources\d2_FG_steel-products.sql
		--:r .\03_Resources\e1_CCE_received-checks.sql
		:r .\03_Resources\h_PL_employee-benefits.sql

		:r .\05_Accounts\a_AccountClassifications.sql
		:r .\05_Accounts\b_BasicAccounts.sql
		:r .\05_Accounts\c_SmartAccounts.sql
		--:r .\00_Security\02_Workflows.sql		

		:r .\06_Entries\00_LineDefinitions.sql
		:r .\06_Entries\01_DocumentDefinitions.sql
		:r .\06_Entries\02_manual-journal-vouchers.sql
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
	ROLLBACK;
RETURN;