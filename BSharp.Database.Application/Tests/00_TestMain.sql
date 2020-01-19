SET NOCOUNT ON;
--ROLLBACK
/* Dependencies
-- Optional screens, can pre-populate table with data
	CustomAccountClassifications -- screen shows list of accounts
	IfrsDisclosures
	MeasurementUnits
	IfrsEntryClassifications
	IfrsAccountClassifications -- screen shows list of accounts
	JobTitles
	Titles, MaritalStatuses, Tribes, Regions, EducationLevels, EducationSublevels, OrganizationTypes, 
-- Critical screens for making a journal entry
	Roles
	Agents, -- screen shows list of relations with Agents
	Resources, -- screen for each ifrs type. Detail shows ResourceInstances
	Accounts
	Workflows, -- screen 
	Documents, -- screen shows Lines, Entries, Signatures, StatesHistory(?)
*/
BEGIN -- reset Identities
	DBCC CHECKIDENT ('[dbo].[Accounts]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[AccountClassifications]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Agents]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Documents]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Lines]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Entries]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[EntryClassifications]', RESEED, 179) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[MeasurementUnits]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Permissions]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[AccountTypes]', RESEED, 72) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Resources]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[RoleMemberships]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Users]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Workflows]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[WorkflowSignatures]', RESEED, 0) WITH NO_INFOMSGS;
END

BEGIN TRY
	BEGIN TRANSACTION
		:r ..\Samples\00_Setup\a_Declarations.sql
		:r ..\Samples\00_Setup\b_RolesMemberships.sql
		:r ..\Samples\00_Setup\z_LookupDefinitions.sql

		:r ..\Samples\01_Basic\a_Currencies.sql
		:r ..\Samples\01_Basic\b_MeasurementUnits.sql
		:r ..\Samples\01_Basic\c_Lookups.sql
		
		:r ..\Samples\02_Agents\01_ResponsibilityCenters.sql
		:r ..\Samples\02_Agents\02_Suppliers.sql
		:r ..\Samples\02_Agents\03_Customers.sql
		:r ..\Samples\02_Agents\04_Employees.sql
		:r ..\Samples\02_Agents\05_Banks.sql
		:r ..\Samples\02_Agents\06_Custodies.sql

		:r ..\Samples\03_Resources\a1_PPE_motor-vehicles.sql
		:r ..\Samples\03_Resources\a2_PPE_it-equipment.sql
		:r ..\Samples\03_Resources\a3_PPE_machineries.sql
		:r ..\Samples\03_Resources\a4_PPE_general-fixed-assets.sql
		:r ..\Samples\03_Resources\b_Inventories_raw-materials.sql
		:r ..\Samples\03_Resources\d1_FG_vehicles.sql

		--:r ..\Samples\03_Resources\d2_FG_steel-products.sql
		--:r ..\Samples\03_Resources\e1_CCE_received-checks.sql
		:r ..\Samples\03_Resources\h_PL_employee-benefits.sql

		:r ..\Samples\05_Accounts\a_AccountClassifications.sql
		:r ..\Samples\05_Accounts\b_BasicAccounts.sql
		:r ..\Samples\05_Accounts\c_SmartAccounts.sql
		--:r .\00_Security\02_Workflows.sql		

		:r ..\Samples\06_Entries\00_LineDefinitions.sql
		:r ..\Samples\06_Entries\01_DocumentDefinitions.sql
		:r ..\Samples\06_Entries\02_manual-journal-vouchers.sql
		--:r ..\Samples\06_Entries\03_cash-payment-vouchers.sql

	IF @DebugAccountTypes = 1
		SELECT
			[AT].Id,
			SPACE(5 * ([AT].[Node].GetLevel() - 1)) +  [AT].[Name] As [Name],
			[AT].[Node].ToString() As [Node],
			[AT].[IsAssignable],
			[AT].[IsActive],
			(SELECT COUNT(*) FROM [AccountTypes] WHERE [ParentNode] = [AT].[Node]) AS [ChildCount]
		FROM dbo.[AccountTypes] [AT]


	IF @DebugAccountTypes = 1
		SELECT * FROM dbo.AccountTypes;

	ROLLBACK;
END TRY
BEGIN CATCH
	ROLLBACK;
	THROW;
END CATCH

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