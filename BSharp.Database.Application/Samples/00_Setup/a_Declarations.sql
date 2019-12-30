	-- Just for debugging convenience. Even though we are roling the transaction, the identities are changing
	DECLARE @ValidationErrorsJson nvarchar(max);
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
	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @ProceedsFromIssuingShares		INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'ProceedsFromIssuingShares' );
	DECLARE @IssueOfEquity					INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'IssueOfEquity' );
	DECLARE @InternalCashTransferExtension	INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InternalCashTransferExtension' );
	DECLARE @InventoryPurchaseExtension		INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InventoryPurchaseExtension' );
	DECLARE @PPEAdditions					INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment' );
	DECLARE @InvReclassifiedAsPPE			INT = (SELECT [Id] FROM dbo.EntryClassifications WHERE [Code] = N'InventoryReclassifiedAsPropertyPlantAndEquipment' );
