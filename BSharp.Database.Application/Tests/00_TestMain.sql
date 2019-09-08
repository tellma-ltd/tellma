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
	DBCC CHECKIDENT ('[dbo].[Agents]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Documents]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLines]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[DocumentLineEntries]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[MeasurementUnits]', RESEED, 100) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Permissions]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ProductCategories]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Resources]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ResourcePicks]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[ResponsibilityCenters]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[RoleMemberships]', RESEED, 1) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[Workflows]', RESEED, 0) WITH NO_INFOMSGS;
	DBCC CHECKIDENT ('[dbo].[WorkflowSignatures]', RESEED, 0) WITH NO_INFOMSGS;

	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @ValidationErrorsJson nvarchar(max);
	DECLARE @DebugIfrsConcepts bit = 0, @DebugMeasurementUnits bit = 0;
	DECLARE @DebugProductCategories bit = 0, @DebugOperations bit = 1, @DebugResources bit = 0;
	DECLARE @DebugAgents bit = 0, @DebugPlaces bit = 0;
	DECLARE @LookupsSelect bit = 0;
	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;
	DECLARE @UserId INT, @RowCount INT;

	SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = '$(DeployEmail)';-- N'support@banan-it.com';
	EXEC sp_set_session_context 'UserId', @UserId;--, @read_only = 1;

	DECLARE @FunctionalCurrency NCHAR(3), @FunctionalCurrencyId INT;
	SELECT @FunctionalCurrency = [FunctionalCurrency] FROM dbo.Settings;
	SELECT @FunctionalCurrencyId = [Id] FROM dbo.[MeasurementUnits] WHERE [Name] = @FunctionalCurrency;
	EXEC sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
END

BEGIN TRY
	BEGIN TRANSACTION
		:r .\01_RolesPermissions.sql		
		:r .\02_Workflows.sql
		:r .\03_MeasurementUnits.sql
		:r .\04_IfrsConcepts.sql
		:r .\05_Agents.sql
		:r .\06_ResponsibilityCenters.sql
		:r .\07_Resources.sql
		:r .\08_Accounts.sql
		:r .\10_JournalVouchers.sql

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