	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @DebugRoles bit = 0,
			@DebugEntryClassifications bit = 0, @DebugAccountTypes bit = 0,
			@DebugLookupDefinitions bit = 0, @DebugRelationDefinitions bit = 0;
	DECLARE @DebugCurrencies bit = 0, @DebugUnits bit = 0, @DebugLookups bit = 0;
	DECLARE @DebugCenters bit = 0;
	DECLARE @DebugSuppliers bit = 0, @DebugCustomers bit = 0, @DebugEmployees bit = 0, @DebugShareholders bit = 0,
			@DebugBanks bit = 0, @DebugCustodies bit = 0, @DebugTaxAgencies bit = 0;
	DECLARE @DebugResources bit = 0, @DebugCustomClassifications bit = 0, @DebugAccounts bit = 0;
	DECLARE @DebugLineDefinitions bit = 0, @DebugDocumentDefinitions bit = 0;
	DECLARE @DebugManualVouchers bit = 1, @DebugReports bit = 0;
	DECLARE @DebugCashPaymentVouchers bit = 0, @DebugPettyCashVouchers bit = 0;
	DECLARE @LookupsSelect bit = 0;

	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;

	DECLARE @RowCount INT;
	DECLARE @ValidationErrorsJson nvarchar(max);

	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @PId INT ;

	DECLARE @Resources dbo.ResourceList, @ResourceUnits dbo.ResourceUnitList;

	DECLARE @BasicSalary INT, @TransportationAllowance INT, @DataPackage INT, @MealAllowance INT, @HourlyWage INT;
	DECLARE @DayOvertime INT, @NightOvertime INT, @RestOvertime INT, @HolidayOvertime INT;
	DECLARE @MonthlySubscription INT;

	DECLARE @SDG NCHAR (3) = N'SDG', @USD NCHAR (3) = N'USD', @SAR NCHAR (3) = N'SAR';

	DECLARE @D dbo.DocumentList, @L dbo.LineList, @E dbo.EntryList, @WL dbo.WideLineList;
	DECLARE @DocsIndexedIds dbo.[IndexedIdList], @LinesIndexedIds dbo.[IndexedIdList];
	DECLARE @Accounts dbo.AccountList;
	DECLARE @LineDefinitions dbo.LineDefinitionList;
	DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
	DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
	DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];

	DECLARE @WorkflowId INT;
	DECLARE @Workflows dbo.[WorkflowList];
	DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;