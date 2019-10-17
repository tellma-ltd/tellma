SET NOCOUNT ON;
--ROLLBACK
/* Dependencies
-- Optional screens, can pre-populate table with data
	CustomAccountClassifications -- screen shows list of accounts
	IfrsDisclosures
	MeasurementUnits
	IfrsEntryClassifications
	IfrsAccountClassifications -- screen shows list of accounts
	AgentRelationTypes
	JobTitles
	Titles, MaritalStatuses, Tribes, Regions, EducationLevels, EducationSublevels, OrganizationTypes, 
-- Critical screens for making a journal entry
	Roles
	Agents, -- screen shows list of relations with Agents
	Resources, -- screen for each ifrs type. Detail shows ResourceInstances
	Accounts
	Workflows, -- screen 
	Documents, -- screen shows Lines, LineEntries, Signatures, StatesHistory(?)
*/
BEGIN -- reset Identities
	DBCC CHECKIDENT ('[dbo].[Accounts]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[AccountClassifications]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Agents]', RESEED, 1) WITH NO_INFOMSGS;
	--DBCC CHECKIDENT ('[dbo].[AgentRelations]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Documents]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLines]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLineEntries]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[MeasurementUnits]', RESEED, 100) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Locations]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Permissions]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ResourceClassifications]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Resources]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ResponsibilityCenters]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[RoleMemberships]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Workflows]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[WorkflowSignatures]', RESEED, 0) WITH NO_INFOMSGS;

	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @ValidationErrorsJson nvarchar(max);
	DECLARE @DebugCurrencies bit = 0, @DebugMeasurementUnits bit = 0;
	DECLARE @DebugResources bit = 0, @DebugAgents bit = 0, @DebugLocations bit = 0, @DebugAccounts INT =0;
	DECLARE @DebugDocuments BIT = 1, @DebugReports BIT = 1;
	DECLARE @LookupsSelect bit = 0;
	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;
	DECLARE @UserId INT, @RowCount INT;

	SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = '$(DeployEmail)';-- N'support@banan-it.com';
	EXEC sp_set_session_context 'UserId', @UserId;--, @read_only = 1;

	DECLARE @FunctionalCurrencyId NCHAR(3);
	SELECT @FunctionalCurrencyId = [FunctionalCurrencyId] FROM dbo.Settings;
	EXEC sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;--, @read_only = 1;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
END

BEGIN TRY
	BEGIN TRANSACTION
		:r .\00_Security\01_RolesPermissions.sql		
		:r .\00_Security\02_Workflows.sql

		:r .\01_Lookups\a_body-colors.sql
		:r .\01_Lookups\b_vehicle-makes.sql
		:r .\01_Lookups\c_steel-thicknesses.sql
		:r .\01_Lookups\d_it-equipment-manufacturers.sql
		:r .\01_Lookups\e_operating-systems.sql
		--select * from lookups;

		:r .\02_Resources\a1_PPE_motor-vehicles.sql
		:r .\02_Resources\a2_PPE_it-equipment.sql
		:r .\02_Resources\a3_PPE_machineries.sql
		:r .\02_Resources\a4_PPE_general-fixed-assets.sql
		:r .\02_Resources\b_Inventories_-raw-materials.sql
		--:r .\02_Resources\d1_FG_vehicles.sql
		:r .\02_Resources\d2_FG_steel-products.sql
		:r .\02_Resources\e1_CCE_cash-assets.sql
		--:r .\02_Resources\e2_CCE_received-checks.sql
		:r .\02_Resources\h_PL_employee-benefits.sql

		:r .\03_Agents\01_Agents.sql
		:r .\03_Agents\02_Suppliers.sql
		:r .\03_Agents\03_Customers.sql
		:r .\03_Agents\04_Employees.sql

		:r .\05_Accounts\00_AccountClassifications.sql
		:r .\05_Accounts\01_gl-accounts.sql
		:r .\05_Accounts\02_tax-accounts.sql

		IF @DebugAccounts = 1
			SELECT * FROM map.Accounts();

		:r .\06_Entries\00_manual-vouchers.sql
		:r .\06_Entries\01_manual-vouchers.sql
		;
		

		--:r .\03_MeasurementUnits.sql
		--:r .\04_IfrsDisclosures.sql
	--	:r .\06_ResponsibilityCenters.sql
		--:r .\07_Resources.sql
		--:r .\08_AccountClassifications.sql
		--:r .\10_JournalVouchers.sql
		--:r .\71_Operations.sql
		--:r .\72_ProductCategories.sql
		--:r .\73_Places.sql

	--	select * from entries;
	--SELECT @fromDate = '2017.01.01', @toDate = '2024.03.01'
	--SELECT * from dbo.[fi_Journal](@fromDate, @toDate);
	--EXEC rpt_TrialBalance @fromDate = @fromDate, @toDate = @toDate, @PrintQuery = 0;


	--SELECT * FROM dbo.[fi_WithholdingTaxOnPayment](default, default);
	--SELECT * FROM dbo.[fi_ERCA__VAT_Purchases](default, default);
	--DECLARE @i int = 0;
	--SELECT @fromDate = '2017.01.1'; SELECT @toDate = DATEADD(DAY, 90, @fromDate);
	--WHILE @i < 30
	--BEGIN
	--	SELECT * FROM [dbo].[fi_AssetRegister](@fromDate, @toDate);
	--	SELECT @fromDate = DATEADD(DAY, 90, @fromDate), @toDate = DATEADD(DAY, 90, @toDate);
	--	SET @i = @i + 1;
	--END
	--SELECT * FROM dbo.[fi_AssetRegister]('2017.02.01', '2018.02.01');
	--SELECT @fromDate = '2017.01.01', @toDate = '2024.01.01';
	--SELECT * FROM dbo.fi_AssetRegister(@fromDate, @toDate);
	--EXEC dbo.rpt_BankAccount__Statement @CBEUSD,  @fromDate, @toDate;
	--SELECT * from dbo.fi_Account__Statement(N'DistributionCosts', @SalesDepartment, @Goff, @fromDate, @toDate) ORDER BY StartDateTime;
	--SELECT * FROM dbo.fi_ERCA__EmployeeIncomeTax('2018.02.01', '2018.03.01');
	--SELECT * FROM dbo.fi_Paysheet(default, default, '2018.02', @Basic, @Transportation);

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