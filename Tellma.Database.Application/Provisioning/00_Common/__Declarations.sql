	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @DebugRoles bit = 0,
			@DebugEntryClassifications bit = 0, @DebugAccountTypes bit = 0,
			@DebugLookupDefinitions bit = 0, @DebugAgentDefinitions bit = 0;
	DECLARE @DebugCurrencies bit = 0, @DebugMeasurementUnits bit = 0, @DebugLookups bit = 0;
	DECLARE @DebugResponsibilityCenters bit = 0;
	DECLARE @DebugSuppliers bit = 0, @DebugCustomers bit = 0, @DebugEmployees bit = 0, @DebugShareholders bit = 0,
			@DebugBanks bit = 0, @DebugCustodies bit = 0, @DebugTaxAgencies bit = 0;
	DECLARE @DebugResources bit = 0, @DebugLegacyClassifications bit = 0, @DebugAccounts bit = 0;
	DECLARE @DebugLineDefinitions bit = 0, @DebugDocumentDefinitions bit = 0;
	DECLARE @DebugManualVouchers bit = 1, @DebugReports bit = 0;
	DECLARE @DebugCashPaymentVouchers bit = 0, @DebugPettyCashVouchers bit = 0;
	DECLARE @LookupsSelect bit = 0;

	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;

	DECLARE @RowCount INT;
	--DECLARE @FunctionalResourceId INT;
	DECLARE @ValidationErrorsJson nvarchar(max);

	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @PId INT ;
	DECLARE @ProceedsFromIssuingShares		INT;
	DECLARE @IssueOfEquity					INT;
	DECLARE @InternalCashTransferExtension	INT;
	DECLARE @InventoryPurchaseExtension		INT;
	DECLARE @PPEAdditions					INT;
	DECLARE @InvReclassifiedAsPPE			INT;
