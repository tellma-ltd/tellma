	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @ValidationErrorsJson nvarchar(max);
	DECLARE @DebugRoles bit = 0, @DebugResourceClassifications bit = 0,
			@DebugEntryClassifications bit = 0, @DebugResourceClassificationsEntryClassifications bit = 0, @DebugAccountTypes bit = 0,
			@DebugLookupDefinitions bit = 0;
	DECLARE @DebugCurrencies bit = 0, @DebugMeasurementUnits bit = 0, @DebugLookups bit = 0;
	DECLARE @DebugResponsibilityCenters bit = 0;
	DECLARE @DebugSuppliers bit = 0, @DebugCustomers bit = 0, @DebugEmployees bit = 0, @DebugShareholders bit = 0,
			@DebugBanks bit = 0, @DebugCustodies bit = 0;
	DECLARE @DebugResources bit = 0, @DebugAccountClassifications bit = 0, @DebugAccounts bit = 0;
	DECLARE @DebugLineDefinitions bit = 0, @DebugDocumentDefinitions bit = 0;
	DECLARE @DebugManualVouchers bit = 1, @DebugReports bit = 0;
	DECLARE @DebugCashPaymentVouchers bit = 0, @DebugPettyCashVouchers bit = 0;
	DECLARE @LookupsSelect bit = 0;
	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;
	DECLARE @UserId INT, @RowCount INT;

	SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = N'admin@bsharp.online';-- '$(DeployEmail)';
	EXEC sp_set_session_context 'UserId', @UserId;--, @read_only = 1;

	DECLARE @FunctionalCurrencyId NCHAR(3), @FunctionalResourceId INT;
	SELECT @FunctionalCurrencyId = [FunctionalCurrencyId] FROM dbo.Settings;
	EXEC sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;--, @read_only = 1;
	SELECT @FunctionalResourceId = [Id] FROM dbo.Resources WHERE DefinitionId = N'currencies' AND CurrencyId = @FunctionalCurrencyId;
	EXEC sp_set_session_context 'FunctionalResourceId', @FunctionalResourceId;--, @read_only = 1;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @ProceedsFromIssuingShares		INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'ProceedsFromIssuingShares' );
	DECLARE @IssueOfEquity					INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'IssueOfEquity' );
	DECLARE @InternalCashTransferExtension	INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InternalCashTransferExtension' );
	DECLARE @InventoryPurchaseExtension		INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InventoryPurchaseExtension' );
	DECLARE @PPEAdditions					INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment' );
	DECLARE @InvReclassifiedAsPPE			INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InventoryReclassifiedAsPropertyPlantAndEquipment' );
