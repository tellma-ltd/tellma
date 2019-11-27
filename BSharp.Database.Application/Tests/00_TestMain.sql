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
	DBCC CHECKIDENT ('[dbo].[Agents]', RESEED, 2) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[AgentRelations]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Documents]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLines]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLineEntries]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[MeasurementUnits]', RESEED, 100) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Permissions]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ResourceClassifications]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Resources]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[RoleMemberships]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Workflows]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[WorkflowSignatures]', RESEED, 0) WITH NO_INFOMSGS;

	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @ValidationErrorsJson nvarchar(max);
	DECLARE @DebugRoles bit = 0, @DebugResourceTypes bit = 0, @DebugEntryTypes bit = 0, @DebugResourceTypesEntryTypes bit = 0, @DebugAccountTypes bit = 1;
	DECLARE @DebugCurrencies bit = 0, @DebugMeasurementUnits bit = 0;
	DECLARE @DebugLookups bit = 0;
	DECLARE @DebugResources bit = 0, @DebugAgents bit = 0, @DebugAccountClassifications bit = 0, @DebugAccounts bit = 0;
	DECLARE @DebugResponsibilityCenters bit = 0;
	DECLARE @DebugManualVouchers bit = 0, @DebugReports bit = 0;
	DECLARE @DebugPettyCashVouchers bit = 1;
	DECLARE @LookupsSelect bit = 0;
	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;
	DECLARE @UserId INT, @RowCount INT;

	SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = N'admin@bsharp.online';-- '$(DeployEmail)';
	EXEC sp_set_session_context 'UserId', @UserId;--, @read_only = 1;

	DECLARE @FunctionalCurrencyId NCHAR(3);
	SELECT @FunctionalCurrencyId = [FunctionalCurrencyId] FROM dbo.Settings;
	EXEC sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;--, @read_only = 1;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
END

BEGIN TRY
	BEGIN TRANSACTION	
		:r ..\Samples\00_Setup\a_RolesMemberships.sql
		:r ..\Samples\00_Setup\b_AgentRelationDefinitions.sql
		:r ..\Samples\00_Setup\c_ResourceTypes.sql
		:r ..\Samples\00_Setup\d_EntryTypes.sql
		:r ..\Samples\00_Setup\e_ResourceTypesEntryTypes.sql
		:r ..\Samples\00_Setup\f_AccountTypes.sql
		--:r ..\Samples\00_Setup\z_LookupDefinitions.sql

		--:r ..\Samples\01_Basic\a_Currencies.sql
		--:r ..\Samples\01_Basic\b_MeasurementUnits.sql
		--:r ..\Samples\01_Basic\c_Lookups.sql
		
		--:r ..\Samples\02_Agents\00_Agents.sql
		--:r ..\Samples\02_Agents\01_ResponsibilityCenters.sql
		--:r ..\Samples\02_Agents\02_Suppliers.sql
		--:r ..\Samples\02_Agents\03_Customers.sql
		--:r ..\Samples\02_Agents\04_Employees.sql

		--:r ..\Samples\03_Resources\a1_PPE_motor-vehicles.sql
		--:r ..\Samples\03_Resources\a2_PPE_it-equipment.sql
		--:r ..\Samples\03_Resources\a3_PPE_machineries.sql
		--:r ..\Samples\03_Resources\a4_PPE_general-fixed-assets.sql
		--:r ..\Samples\03_Resources\b_Inventories_raw-materials.sql
		--:r ..\Samples\03_Resources\d1_FG_vehicles.sql
		--:r ..\Samples\03_Resources\d2_FG_steel-products.sql
		--:r ..\Samples\03_Resources\e1_CCE_received-checks.sql
		--:r ..\Samples\03_Resources\h_PL_employee-benefits.sql

		--:r ..\Samples\05_Accounts\a_AccountGroups.sql
		--:r ..\Samples\05_Accounts\b_AccountClassifications.sql
		--:r ..\Samples\05_Accounts\c_gl-accounts.sql
		--:r .\05_Accounts\02_tax-accounts.sql
		--:r .\00_Security\02_Workflows.sql		


		--:r ..\Samples\06_Entries\00_manual-vouchers.sql
		--:r .\06_Entries\01_petty-cash-vouchers.sql
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