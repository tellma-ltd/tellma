INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(0, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(71, N'ProjectCompletionToPropertyPlantAndEquipment', N'real estate project turning into properties to use', N'For Use', N'For Use', 0, 0),
(72, N'ProjectCompletionToInventory', N'real estate project turning into properties for sale', N'For Sale', N'For Sale', 0, 0),
(73, N'ProjectCompletionToInvestmentProperty', N'real estate project turning into properties for rent', N'For Rent', N'For Rent', 0, 0),
(81, N'PPEDepreciation', N'Depreciating assets that are time based, and using the number of days as criteria', N'Depreciation', N'Assets Depreciation', 0, 0),
(82, N'IntangibleAmortization', N'', N'Amortization', N'Amortization', 0, 0),
(83, N'ExchangeVariance', N'', N'Exchange Variance', N'Exchange Variances', 0, 0),
(84, N'TradeSettlement', N'Adjusting trade payables and trade receivables balances', N'Settlement', N'Settlements', 0, 0),
(85, N'Hyperinflation', N'Adjusting according to IAS 29', N'Hyperinflation', N'Hyperinflation', 0, 0),
(91, N'CostReallocationToConstructionInProgress', N'Capitalization of a project expenditures', N'Project', N'Projects', 0, 0),
(92, N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment', N'Capitalization of an investment property expenditures ', N'Investment Property', N'Investment Properties', 0, 0),
(93, N'CostReallocationToCurrentInventoriesInTransit', N'Capitalization of expenditures on inventories in transit', N'Goods In Transit', N'Goods In Transit', 0, 0),
(100, N'CashTransferExchange', N'cash transfer and currency exchange', N'Transfer & Exchange', N'Cash Transfers', 0, 1),
(101, N'CashTransfer', N'cash transfer, same currency', N'Transfer', N'Transfers', 0, 1),
(102, N'CashExchange', N'currency exchange, same account', N'Exchange', N'Exchanges', 0, 1),
(110, N'DepositCashToBank', N'deposit cash in bank', N'Cash Deposit', N'Cash Deposits', 0, 1),
(111, N'DepositCheckToBank', N'deposit checks in bank', N'Check Deposit', N'Check Deposits', 0, 0),
(120, N'CashReceiptFromOtherToCashier', N'cash receipt by cashier or bank from other than customers, suppliers or employees', N'Cash Payment', N'Cash Payments', 0, 1),
(121, N'CheckReceiptFromOtherToCashier', N'check receipt by cashier from other than customers, suppliers or employees', N'Check Payment', N'Check Payments', 0, 1),
(130, N'CashPaymentToOther', N'cash payment to other than suppliers, customers, and employees', N'Payment to Other', N'Payments to Others', 0, 1),
(300, N'CashPaymentToTradePayable', N'issuing Payment to supplier/lessor/..', N'Payment', N'Payments', 0, 1),
(301, N'InvoiceFromTradePayable', N'Receiving Invoice from supplier/lessor', N'Invoice', N'Invoices', 0, 1),
(302, N'StockReceiptFromTradePayable', N'Receiving goods to inventory from supplier/contractor', N'Stock', N'Stock', 0, 0),
(303, N'PPEReceiptFromTradePayable', N'Receiving property, plant and equipment from supplier/contractor', N'Fixed Asset', N'Fixed Assets', 0, 1),
(304, N'ConsumableServiceReceiptFromTradePayable', N'Receiving services/consumables from supplier/lessor/consultant, ...', N'Consumable - Service', N'Consumables - Services', 0, 1),
(305, N'RentalReceiptFromTradePayable', N'Receiving rental service from lessor', N'Rental', N'Rentals', 0, 1),
(310, N'CashPaymentFromTradePayable', N'refund', N'Refund', N'Refunds', 0, 1),
(400, N'CashReceiptFromTradeReceivable', N'Receiving cash payment from customer/lessee', N'Cash', N'Cash', 0, 1),
(401, N'CheckReceiptFromTradeReceivable', N'Receiving check payment from customer/lessee', N'check', N'Checks', 0, 1),
(402, N'InvoiceToTradeReceivable', N'Issuing invoice to customer/lessee', N'Invoice', N'Invoices', 0, 1),
(403, N'StockIssueToTradeReceivable', N'Issuing stock to customer', N'Stock', N'Stock', 0, 0),
(404, N'ServiceDeliveryToTradeReceivable', N'Delivering service to customer', N'Service', N'Services', 0, 0);

--0: ManualLine
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [AccountTypeId]) VALUES (0,0,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,0,	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Memo',		0,			N'Memo',		4,4,1);
--INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
--[State],	[Name]) VALUES
--(0,0,-4,	N'Duplicate Line'),
--(1,0,-4,	N'Incorrect Analysis'),
--(2,0,-4,	N'Other reasons'); -- @
--INSERT INTO @Workflows([Index],[LineDefinitionIndex],
--[ToState]) Values
--(0,0,+3),
--(1,0,+4);
--INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
--[RuleType],			[RoleId]) VALUES
--(0,0,0,N'ByRole',	@ComptrollerRL),
--(0,1,0,N'ByRole',	@FinanceManagerRL);
PRINT N'';
--72:ProjectCompletionToInventory
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1] = [MonetaryValue0], -- TODO: When we add V29, we need to change this
		[CurrencyId1] = [CurrencyId0]
'
WHERE [Index] = 72;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,72,+1,	@PropertyIntendedForSaleInOrdinaryCourseOfBusiness, @IncreaseDecreaseThroughProductionExtension),
(1,72,-1,	@InvestmentPropertyUnderConstructionOrDevelopment, NULL)
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,72,	N'ResourceId',			0,	N'Property',		1,2,0),
(1,72,	N'MonetaryValue',		0,	N'Cost',			1,2,0),
(2,72,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,72,	N'CenterId',			0,	N'Business Unit',	1,4,1),
(4,72,	N'CenterId',			1,	N'Project',			1,4,1),
(6,72,	N'PostingDate',			0,	N'Posting Date',	4,4,1),
(7,72,	N'Memo',				0,	N'Memo',			1,2,1);
--83: ExchangeVariance
UPDATE @LineDefinitions -- Assumes only one foreign currency if we have several foreign currency accounts, we need to pass a collection of the currencies and their rates
SET [GenerateScript] = N'
		DECLARE @CurrencyId NCHAR (3), @ExchangeRate DECIMAL (19,6), @PostingDate DATE;
		
		DECLARE @WideLines WideLineList;
				
		SELECT @CurrencyId = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''CurrencyId'') AS NCHAR (3));
		SELECT @ExchangeRate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''ExchangeRate'') AS DECIMAL (19,6));
		SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);

		DECLARE @FunctionalCurrencyId NCHAR (3) = [dbo].[fn_FunctionalCurrencyId]();
		DECLARE @LocalCurrencyId NCHAR (3);
		SELECT @LocalCurrencyId = [Id] FROM dbo.Currencies WHERE [IsActive] = 1 AND [Id] <> @FunctionalCurrencyId;

		INSERT INTO @WideLines (
			[Index],
			[Memo],
			[PostingDate],
			[CurrencyId0],
			[CenterId0],
			[MonetaryValue0],
			[Value0]
		)
		SELECT
			0,
			@CurrencyId  + N'' Exchange variance @ '' + CAST(@ExchangeRate AS NVARCHAR (10)),
			@PostingDate,
			@LocalCurrencyId,
			NULL,
			-SUM([Direction] * [MonetaryValue] * IIF(E.CurrencyId = @FunctionalCurrencyID, @ExchangeRate, 1)),
			0
		FROM dbo.Entries E 
		JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		HAVING SUM([Direction] * [MonetaryValue] * IIF(E.CurrencyId = @FunctionalCurrencyID, @ExchangeRate, 1)) <> 0;

		SELECT * FROM @WideLines;
	'
WHERE [Index] = 83;
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],					[Visibility],	[DataType],	[Filter]) VALUES
(0,83,N'CurrencyId',	N'Foreign Currency',		N'Required',	N'Currency',NULL),
(1,83,N'ExchangeRate',	N'Official Exchange Rate',	N'Required',	N'Decimal', NULL),
(2,83,N'PostingDate',	N'As Of Date',				N'Required',	N'Date',	NULL);
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,83,	+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,83,	N'Memo',				0,	N'Memo',				4,4,0),
(2,83,	N'MonetaryValue',		0,	N'Gain (Loss)',			4,4,0),
(4,83,	N'PostingDate',			0,	N'Posting Date',		4,4,1),
(6,83,	N'CenterId',			0,	N'Business Unit',		4,4,0);
--91: CostReallocationToConstructionInProgress
UPDATE @LineDefinitions
SET [GenerateScript] = N'
		DECLARE @CenterId INT, @PostingDate DATE;
		
		DECLARE @WideLines WideLineList;
				
		SELECT @CenterId = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''CenterId'') AS INT);
		SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);
		DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N''ExpenseByNature'');

		WITH ExpenseByNatureAccounts AS (
			SELECT A.[Id]
			FROM dbo.Accounts A
			JOIN dbo.AccountTypes [ATC] ON A.[AccountTypeId] = [ATC].[Id]
			WHERE [ATC].[Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
		INSERT INTO @WideLines(
			[Index], [PostingDate],
			[AccountId0], [CurrencyId0], [ContractId0], [ResourceId0], [UnitId0], [EntryTypeId0], [DueDate0], [Centerid0], [Quantity0], [MonetaryValue0], [Value0],
			[AccountId1], [CurrencyId1], [ContractId1], [ResourceId1], [UnitId1], [EntryTypeId1], [DueDate1], [Centerid1], [Quantity1], [MonetaryValue1], [Value1]
		)
		SELECT
			ROW_NUMBER() OVER(ORDER BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]) - 1 AS [Index], 
			@PostingDate,
			NULL			AS [AccountId0],
			[CurrencyId]	AS [CurrencyId0],
			NULL			AS [ContractId0],
			NULL			AS [ResourceId0],
			NULL			AS [UnitId0],
			NULL			AS [EntryTypeId0],
			NULL			As [DueDate0],
			@CenterId		AS [Centerid0],
			SUM([Direction] * [Quantity]) AS [Quantity0],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue0],
			SUM([Direction] * [Value]) AS [Value0],

			[AccountId]		AS [AccountId1],
			[CurrencyId]	AS [CurrencyId1],
			[ContractId]	AS [ContractId1],
			[ResourceId]	AS [ResourceId1],
			[UnitId]		AS [UnitId1],
			[EntryTypeId]	AS [EntryTypeId1],
			[DueDate]		As [DueDate1],
			@CenterId		AS [Centerid1],
			SUM([Direction] * [Quantity]) AS [Quantity1],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue1],
			SUM([Direction] * [Value]) AS [Value1]
		FROM dbo.Entries E 
		JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE E.[CenterId] = @CenterId
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]

		SELECT * FROM @WideLines;
	'
WHERE [Index] = 91;
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],		[Visibility],	[DataType],	[Filter]) VALUES
(0,91,N'CenterId',		N'Project',		N'Required',	N'Center',	N'CenterType = ''ProductionExpenseControl'''),
(1,91,N'PostingDate',	N'As Of Date',	N'Required',	N'Date',	NULL);
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,91,	+1, @ConstructionInProgress),
(1,91,	-1, @ExpenseByNature);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,91,	N'AccountId',			1,	N'Expenditure',			4,4,0),
(1,91,	N'ResourceId',			1,	N'Item',				5,5,0),
(2,91,	N'Quantity',			1,	N'Quantity',			5,5,0),
(3,91,	N'UnitId',				1,	N'Unit',				5,5,0),
(4,91,	N'MonetaryValue',		1,	N'Amount',				4,4,0),
(5,91,	N'CurrencyId',			1,	N'Currency',			4,4,0),
(6,91,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(7,91,	N'Memo',				1,	N'Memo',				4,4,1),
(8,91,	N'CenterId',			1,	N'Project',				4,4,1);
--92: CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment
UPDATE @LineDefinitions
SET [GenerateScript] = N'
		DECLARE @CenterId INT, @PostingDate DATE;
		
		DECLARE @WideLines WideLineList;
				
		SELECT @CenterId = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''CenterId'') AS INT);
		SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);
		DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N''ExpenseByNature'');

		WITH ExpenseByNatureAccounts AS (
			SELECT A.[Id]
			FROM dbo.Accounts A
			JOIN dbo.AccountTypes [ATC] ON A.[AccountTypeId] = [ATC].[Id]
			WHERE [ATC].[Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
		INSERT INTO @WideLines(
			[Index], [PostingDate],
			[AccountId0], [CurrencyId0], [ContractId0], [ResourceId0], [UnitId0], [EntryTypeId0], [DueDate0], [Centerid0], [Quantity0], [MonetaryValue0], [Value0],
			[AccountId1], [CurrencyId1], [ContractId1], [ResourceId1], [UnitId1], [EntryTypeId1], [DueDate1], [Centerid1], [Quantity1], [MonetaryValue1], [Value1]
		)
		SELECT
			ROW_NUMBER() OVER(ORDER BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]) - 1 AS [Index], 
			@PostingDate,
			NULL			AS [AccountId0],
			[CurrencyId]	AS [CurrencyId0],
			NULL			AS [ContractId0],
			NULL			AS [ResourceId0],
			NULL			AS [UnitId0],
			NULL			AS [EntryTypeId0],
			NULL			As [DueDate0],
			@CenterId		AS [Centerid0],
			SUM([Direction] * [Quantity]) AS [Quantity0],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue0],
			SUM([Direction] * [Value]) AS [Value0],

			[AccountId]		AS [AccountId1],
			[CurrencyId]	AS [CurrencyId1],
			[ContractId]	AS [ContractId1],
			[ResourceId]	AS [ResourceId1],
			[UnitId]		AS [UnitId1],
			[EntryTypeId]	AS [EntryTypeId1],
			[DueDate]		As [DueDate1],
			@CenterId		AS [Centerid1],
			SUM([Direction] * [Quantity]) AS [Quantity1],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue1],
			SUM([Direction] * [Value]) AS [Value1]
		FROM dbo.Entries E 
		JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE E.[CenterId] = @CenterId
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]

		SELECT * FROM @WideLines;
	'
WHERE [Index] = 92;
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],		[Visibility],	[DataType],	[Filter]) VALUES
(0,92,N'CenterId',		N'Scheme',		N'Required',	N'Center',	N'CenterType = ''ConstructionExpenseControl'''),
(1,92,N'PostingDate',	N'As Of Date',	N'Required',	N'Date',	NULL);
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,92,	+1, @InvestmentPropertyUnderConstructionOrDevelopment),
(1,92,	-1, @ExpenseByNature);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,92,	N'AccountId',			1,	N'Expenditure',			4,4,0),
(1,92,	N'ResourceId',			1,	N'Item',				5,5,0),
(2,92,	N'Quantity',			1,	N'Quantity',			5,5,0),
(3,92,	N'UnitId',				1,	N'Unit',				5,5,0),
(4,92,	N'MonetaryValue',		1,	N'Amount',				4,4,0),
(5,92,	N'CurrencyId',			1,	N'Currency',			4,4,0),
(6,92,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(7,92,	N'Memo',				1,	N'Memo',				4,4,1),
(8,92,	N'CenterId',			1,	N'Scheme',				4,4,1);
--93: CostReallocationToCurrentInventoriesInTransit
UPDATE @LineDefinitions
SET [GenerateScript] = N'
		DECLARE @CenterId INT, @PostingDate DATE;
		
		DECLARE @WideLines WideLineList;
				
		SELECT @CenterId = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''CenterId'') AS INT);
		SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);
		DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N''ExpenseByNature'');

		WITH ExpenseByNatureAccounts AS (
			SELECT A.[Id]
			FROM dbo.Accounts A
			JOIN dbo.AccountTypes [ATC] ON A.[AccountTypeId] = [ATC].[Id]
			WHERE [ATC].[Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
		INSERT INTO @WideLines(
			[Index], [PostingDate],
			[AccountId0], [CurrencyId0], [ContractId0], [ResourceId0], [UnitId0], [EntryTypeId0], [DueDate0], [Centerid0], [Quantity0], [MonetaryValue0], [Value0],
			[AccountId1], [CurrencyId1], [ContractId1], [ResourceId1], [UnitId1], [EntryTypeId1], [DueDate1], [Centerid1], [Quantity1], [MonetaryValue1], [Value1]
		)
		SELECT
			ROW_NUMBER() OVER(ORDER BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]) - 1 AS [Index], 
			@PostingDate,
			NULL			AS [AccountId0],
			[CurrencyId]	AS [CurrencyId0],
			NULL			AS [ContractId0],
			NULL			AS [ResourceId0],
			NULL			AS [UnitId0],
			NULL			AS [EntryTypeId0],
			NULL			As [DueDate0],
			@CenterId		AS [Centerid0],
			SUM([Direction] * [Quantity]) AS [Quantity0],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue0],
			SUM([Direction] * [Value]) AS [Value0],

			[AccountId]		AS [AccountId1],
			[CurrencyId]	AS [CurrencyId1],
			[ContractId]	AS [ContractId1],
			[ResourceId]	AS [ResourceId1],
			[UnitId]		AS [UnitId1],
			[EntryTypeId]	AS [EntryTypeId1],
			[DueDate]		As [DueDate1],
			@CenterId		AS [Centerid1],
			SUM([Direction] * [Quantity]) AS [Quantity1],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue1],
			SUM([Direction] * [Value]) AS [Value1]
		FROM dbo.Entries E 
		JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE E.[CenterId] = @CenterId
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]

		SELECT * FROM @WideLines;
	'
WHERE [Index] = 93;
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],		[Visibility],	[DataType],	[Filter]) VALUES
(0,93,N'CenterId',		N'Shipment',		N'Required',	N'Center',	N'CenterType = ''TransitExpenseControl'''),
(1,93,N'PostingDate',	N'As Of Date',	N'Required',	N'Date',	NULL);
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,93,	+1, @CurrentInventoriesInTransit),
(1,93,	-1, @ExpenseByNature);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,93,	N'AccountId',			1,	N'Expenditure',			4,4,0),
(1,93,	N'ResourceId',			1,	N'Item',				5,5,0),
(2,93,	N'Quantity',			1,	N'Quantity',			5,5,0),
(3,93,	N'UnitId',				1,	N'Unit',				5,5,0),
(4,93,	N'MonetaryValue',		1,	N'Amount',				4,4,0),
(5,93,	N'CurrencyId',			1,	N'Currency',			4,4,0),
(6,93,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(7,93,	N'Memo',				1,	N'Memo',				4,4,1),
(8,93,	N'CenterId',			1,	N'Shipment',			4,4,1);
--100:CashPaymentToOther
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 100;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,100,	+1, @CashPaymentsToOthersControlExtension),
(1,100,	-1, @CashAndCashEquivalents);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,100,	N'Memo',				1,	N'Memo',				1,2,1),
(1,100,	N'CurrencyId',			1,	N'Currency',			1,2,1),
(2,100,	N'MonetaryValue',		1,	N'Pay Amount',			1,2,0),
(3,100,	N'NotedAgentName',		1,	N'Beneficiary',			3,3,0),
(4,100,	N'ContractId',			1,	N'Bank/Cashier',		3,3,0),
(5,100,	N'ExternalReference',	1,	N'Check #/Receipt #',	3,3,0),
(6,100,	N'NotedDate',			1,	N'Check Date',			3,3,0),
(7,100,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(8,100,	N'EntryTypeId',			1,	N'Purpose',				4,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name]) VALUES
(0,100,-3,	N'Insufficient Balance'),
(1,100,-3,	N'Other reasons');
--104:CashTransferExchange => make into a separate document with transfer only, exchange only, exchange and transfer
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId0] = COALESCE((SELECT [CenterId] FROM dbo.Contracts WHERE [Id] = [ContractId0]), [CenterId2]),
		[CenterId1] = COALESCE((SELECT [CenterId] FROM dbo.Contracts WHERE [Id] = [ContractId1]), [CenterId2]),
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 104;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,104,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,104,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,104,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,104,	N'ContractId',			1,	N'From Account',	1,2,0),
(1,104,	N'ContractId',			0,	N'To Account',		1,2,0),
(2,104,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,104,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,104,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,104,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,104,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(7,104,	N'Memo',				0,	N'Memo',			1,2,1);
--110:DepositCashToBank -- Make this and the next into a separate document (BankDepositVoucher)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1]
'
WHERE [Index] = 110;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],	[EntryTypeId]) VALUES
(0,110,+1,	@BalancesWithBanks,	@InternalCashTransferExtension),
(1,110,-1,	@CashOnHand,		@InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,110,	N'ContractId',			1,	N'From Cash Account',1,2,0),
(1,110,	N'ContractId',			0,	N'To Bank Account',	1,2,0),
(2,110,	N'CurrencyId',			1,	N'Currency',		1,2,0),
(3,110,	N'MonetaryValue',		1,	N'Amount',			1,3,0),
(4,110,	N'PostingDate',			1,	N'Posting Date',	4,4,1),
(5,110,	N'Memo',				0,	N'Memo',			1,2,1);
--111:DepositCheckToBank
UPDATE @LineDefinitions
SET [GenerateScript] = N'
		DECLARE @ContractId0 INT, @ContractId1 INT, @PostingDate DATE;
		
		DECLARE @WideLines WideLineList;
				
		SELECT @ContractId0 = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''ContractId0'') AS INT);
		SELECT @ContractId1 = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''ContractId1'') AS INT);
		SELECT @PostingDate = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N''PostingDate'') AS DATE);

		DECLARE @CurrencyId NCHAR (3) = (SELECT [CurrencyId] FROM dbo.Contracts WHERE [Id] = @ContractId0);
		DECLARE @EntryTypeId INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'''');
		WITH CheckOnHandAccounts AS
		(
			SELECT A.[Id] FROM dbo.Accounts A
			JOIN dbo.AccountTypes ATC ON A.AccountTypeId = ATC.[Id]
			JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
			WHERE ATP.[Concept] = N''CashOnHand''
		)
		INSERT INTO @WideLines(
			[Index], [PostingDate],
			[AccountId0], [CurrencyId0], [ContractId0], [ResourceId0], [UnitId0], [EntryTypeId0], [DueDate0], [Centerid0], [Quantity0], [MonetaryValue0], [Value0],[ExternalReference0],
			[AccountId1], [CurrencyId1], [ContractId1], [ResourceId1], [UnitId1], [EntryTypeId1], [DueDate1], [Centerid1], [Quantity1], [MonetaryValue1], [Value1]
		)
		SELECT
			ROW_NUMBER() OVER(ORDER BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate]) - 1 AS [Index], 
			@PostingDate,
			NULL			AS [AccountId0],
			@CurrencyId		AS [CurrencyId0],
			@ContractId0	AS [ContractId0],
			NULL			AS [ResourceId0],
			NULL			AS [UnitId0],
			@EntryTypeId	AS [EntryTypeId0],
			NULL			As [DueDate0],
			NULL			AS [CenterId0],
			NULL			AS [Quantity0],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue0],
			SUM(E.[Direction] * E.[Value]) AS [Value0],
			R.[Text1]		AS [ExternalReference0],

			E.[AccountId]		AS [AccountId1],
			E.[CurrencyId]	AS [CurrencyId1],
			E.[ContractId]	AS [ContractId1],
			E.[ResourceId]	AS [ResourceId1],
			E.[UnitId]		AS [UnitId1],
			@EntryTypeId	AS [EntryTypeId1],
			E.[DueDate]		As [DueDate1],
			NULL			AS [CenterId1],
			NULL			AS [Quantity1],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue1],
			SUM(E.[Direction] * E.[Value]) AS [Value1]
		FROM dbo.Entries E 
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Resources R ON E.ResourceId = R.[Id]
		WHERE E.[ContractId] = @ContractId1
		AND E.[AccountId] IN (SELECT [Id] FROM CheckOnHandAccounts)
		AND E.[CurrencyId] = @CurrencyId
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[CurrencyId], E.[ContractId], E.[ResourceId], E.[UnitId], E.[EntryTypeId], E.[DueDate], R.[text1]

		SELECT * FROM @WideLines;
	'
WHERE [Index] = 111;
INSERT INTO @LineDefinitionGenerateParameters([Index], [HeaderIndex],
		[Key],			[Label],				[Visibility],	[DataType],		[Filter]) VALUES
(0,111,N'ContractId0',	N'Bank Account',		N'Required',	N'Contract/' + CAST (@BankAccountCD AS NVARCHAR(50)),	NULL),
(1,111,N'ContractId1',	N'Cashier',				N'Required',	N'Contract/' + CAST (@CashOnHandAccountCD AS NVARCHAR(50)),	NULL),
(2,111,N'PostingDate',	N'As Of Date',			N'Required',	N'Date',	NULL);
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CurrencyId0] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[MonetaryValue0] = (SELECT [MonetaryValue] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[MonetaryValue1] = (SELECT [MonetaryValue] FROM dbo.Resources WHERE [Id] = [ResourceId1])
		-- Add the checkinfo to the bank line
'
WHERE [Index] = 111;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],	[EntryTypeId]) VALUES
(0,111,+1,	@BalancesWithBanks, @InternalCashTransferExtension),
(1,111,-1,	@CashOnHand,		@InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,111,	N'ContractId',			1,	N'From Cash Account',1,2,0),
(1,111,	N'ContractId',			0,	N'To Bank Account',	1,2,0),
(2,111,	N'ResourceId',			1,	N'Check Received',	1,2,0),
(4,111,	N'PostingDate',			1,	N'Posting Date',	4,4,1),
(5,111,	N'Memo',				0,	N'Memo',			1,2,1);
--120:CashReceiptFromOther
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 120;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,120,	+1, @CashAndCashEquivalents),
(1,120,	-1, @CashReceiptsFromOthersControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,120,	N'Memo',				0,	N'Memo',				1,2,1),
(1,120,	N'CurrencyId',			0,	N'Currency',			1,2,1),
(2,120,	N'MonetaryValue',		0,	N'Amount Received',		1,2,0),
(3,120,	N'NotedAgentName',		0,	N'From',				3,3,0),
(4,120,	N'ContractId',			0,	N'Bank/Cashier',		3,3,0),
(5,120,	N'ExternalReference',	0,	N'Receipt #',			3,3,0),
(6,120,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(8,120,	N'EntryTypeId',			0,	N'Purpose',				4,4,0);
--121:CheckReceiptFromOtherInCashier
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[MonetaryValue0] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[MonetaryValue1] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 121;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,121,	+1, @CashOnHand),
(1,121,	-1, @CashReceiptsFromOthersControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,121,	N'Memo',				0,	N'Memo',				1,2,1),
(1,121,	N'ResourceId',			0,	N'Check',				1,1,1),
(2,121,	N'CurrencyId',			0,	N'Currency',			1,1,1),
(3,121,	N'MonetaryValue',		0,	N'Amount Received',		1,1,0),
(4,121,	N'NotedAgentName',		0,	N'From',				3,3,0),
(5,121,	N'ContractId',			0,	N'Cashier',				3,3,0),
(6,121,	N'ExternalReference',	0,	N'Receipt #',			3,3,0),
(7,121,	N'PostingDate',			1,	N'Posting Date',		4,4,1),
(8,121,	N'EntryTypeId',			0,	N'Purpose',				4,4,0);
--300:CashPaymentToTradePayable: Supplier (=> Cash Purchase Voucher),-- CashPaymentToEmployee (=> Employee Payment Voucher),-- CashPaymentToCustomer (=> Customer refund Voucher)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId0]			= [CenterId1]
'
WHERE [Index] = 300;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,300,+1,	@CashPaymentsToSuppliersControlExtension, NULL),
(1,300,-1,	@CashAndCashEquivalents, @PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,300,	N'Memo',				1,	N'Memo',			1,4,1),
(1,300,	N'ContractId',			0,	N'Supplier',		3,4,1),
(2,300,	N'CurrencyId',			0,	N'Invoice Currency',1,2,1),
(3,300,	N'MonetaryValue',		0,	N'Invoice Amount',	1,2,0),
(4,300,	N'ContractId',			1,	N'Bank/Cashier',	3,4,0),
(5,300,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(6,300,	N'NotedDate',			1,	N'Check Date',		4,4,0),
(7,300,	N'PostingDate',			1,	N'Paid On',			1,4,1);
--400:CashReceiptFromTradeReceivable
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]);

	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1] = [bll].[fn_ConvertCurrencies](PostingDate, CurrencyId0, CurrencyId1, MonetaryValue0);

'
WHERE [Index] = 400;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,400,+1,	@CashOnHand,		@ReceiptsFromSalesOfGoodsAndRenderingOfServices),-- cashier
(1,400,-1,	@CashReceiptsFromCustomersControlExtension, NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,400,	N'Memo',				0,	N'Memo',			1,4,1),
(1,400,	N'ContractId',			1,	N'Customer',		1,4,1),
(2,400,	N'ContractId',			0,	N'Cashier',			3,4,1),
(3,400,	N'PostingDate',			0,	N'Paid On',			1,4,1),
(4,400,	N'CurrencyId',			0,	N'Payment Currency',1,2,0),
(5,400,	N'MonetaryValue',		0,	N'Payment Amount',	1,2,0);

--401:CheckReceiptFromTradeReceivable
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0] = (SELECT [MonetaryValue] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CurrencyId0] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]);

	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1] = [bll].[fn_ConvertCurrencies](PostingDate, CurrencyId0, CurrencyId1, MonetaryValue0);
'
WHERE [Index] = 401;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,401,+1,	@CashOnHand,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices), -- only in cashier
(1,401,-1,	@CashReceiptsFromCustomersControlExtension, NULL);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,0,401,@CheckReceivedRD)
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,401,	N'Memo',				0,	N'Memo',			1,4,1),
(1,401,	N'ContractId',			1,	N'Customer',		1,3,1),
(2,401,	N'ContractId',			0,	N'Cashier',			3,4,1),
(3,401,	N'PostingDate',			0,	N'Paid On',			1,4,1),
(4,401,	N'ResourceId',			0,	N'Check',			3,4,0);
GOTO DONE_LD
--3:PaymentToEmployee (used in a payroll voucher)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]	= [CurrencyId0],
		[NotedContractId1] = [ContractId0],
		[MonetaryValue1]= [MonetaryValue0],
		[CenterId0]		= [CenterId1]
'
WHERE [Index] = 3;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
-- We might better add ContractDefinitionId and limit it to employees for this case
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,3,+1,	@OtherCurrentPayables, NULL),
(1,3,-1,	@CashAndCashEquivalents, @PaymentsToAndOnBehalfOfEmployees);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,3,	N'Memo',				0,	N'Memo',			1,4,1),
(1,3,	N'ContractId',			0,	N'Employee',		3,4,1),
(2,3,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,3,	N'MonetaryValue',		0,	N'Amount',			1,2,0),
(4,3,	N'ContractId',			1,	N'Bank/Cashier',	3,4,0),
(5,3,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(6,3,	N'NotedDate',			1,	N'Check Date',		5,4,0),
(7,3,	N'CenterId',			1,	N'Inv. Ctr',		4,4,1);
--11:StockReceiptCreditPurchase (inv-gs,cash) [rarely used in ET]
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 11;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],				[EntryTypeId]) VALUES
(0,11,+1,	@Inventories,					@ReceiptsReturnsThroughPurchaseExtension), -- 
(1,11,-1,	@CurrentValueAddedTaxReceivables,NULL), 
(2,11,-1,	@GoodsAndServicesReceivedFromSuppliersControlExtensions,		NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,11,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,11,	N'ResourceId',			0,	N'Item',			N'الصنف',			3,4,0),
(2,11,	N'Quantity',			0,	N'Quantity',		N'الكمية',			1,2,0),
(3,11,	N'UnitId',				0,	N'Unit',			N'الوحدة',			1,2,0),
(4,11,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	N'السعر (بلا ق.م.)',1,2,0),
(5,11,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',			1,2,0),
(6,11,	N'MonetaryValue',		2,	N'Price (w/ VAT)',	N'السعر (مع ق.م.)',1,2,0),
(7,11,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(8,11,	N'ContractId',			0,	N'Warehouse',		N'المخزن',			3,3,1),
(9,11,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(10,11,	N'NotedContractId',		0,	N'Supplier',		N'المورد',			3,3,1);
/*
--12:StockReceiptPurchase (inv-cash-gs),  (inv-cash,gs), (gs,inv-cash)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 12;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,12,+1,	@ReceiptsReturnsThroughPurchaseExtension), -- @Inventories
(1,12,-1,	NULL); -- @TradingControlExtension
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,12,	@RawMaterials),
(1,0,12,	@ProductionSupplies),
(2,0,12,	@Merchandise),
(3,0,12,	@CurrentFoodAndBeverage),
(4,0,12,	@CurrentAgriculturalProduce),
(5,0,12,	@FinishedGoods),
(6,0,12,	@CurrentPackagingAndStorageMaterials),
(7,0,12,	@SpareParts),
(8,0,12,	@CurrentFuel),
(9,0,12,	@PropertyIntendedForSaleInOrdinaryCourseOfBusiness),
(10,0,12,	@OtherInventories),
(0,1,12,	@TradingControlExtension);
--13:ConsumableServiceReceiptCreditPurchase (inv-gs,cash) [rarely used, applies to travel expenses]
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 13;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,13,+1,	NULL), -- @ExpenseByNature
(1,13,-1,	NULL), -- @CurrentValueAddedTaxReceivables
(2,13,-1,	NULL); -- @TradeAndOtherCurrentPayablesToTradeSuppliers
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,13,	@RawMaterialsAndConsumablesUsed),
(1,0,13,	@CostOfMerchandiseSold),
(2,0,13,	@InsuranceExpense),
(3,0,13,	@ProfessionalFeesExpense),
(4,0,13,	@TransportationExpense),
(5,0,13,	@BankAndSimilarCharges),
(6,0,13,	@TravelExpense),
(7,0,13,	@CommunicationExpense),
(8,0,13,	@UtilitiesExpense),
(9,0,13,	@AdvertisingExpense),
(10,0,13,	@WagesAndSalaries),
(11,0,13,	@SocialSecurityContributions),
(12,0,13,	@OtherShorttermEmployeeBenefits),
(13,0,13,	@EmployeeBonusExtension),
(14,0,13,	@PostemploymentBenefitExpenseDefinedContributionPlans),
(15,0,13,	@PostemploymentBenefitExpenseDefinedBenefitPlans),
(16,0,13,	@TerminationBenefitsExpense),
(17,0,13,	@OtherLongtermBenefits),
(18,0,13,	@OtherEmployeeExpense),
(19,0,13,	@OtherExpenseByNature),
(0,1,13,	@CurrentValueAddedTaxReceivables),
(0,2,13,	@TradeAndOtherCurrentPayablesToTradeSuppliers);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,13,	N'Memo',				0,	N'Memo',			1,2,1),
(1,13,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,13,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,13,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,13,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,13,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--14:ConsumableServiceReceiptPurchase (inv-cash-gs) (inv-cash,gs) (gs,inv-cash), can used for LeaseIn as well...
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 14;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,14,+1,	NULL), -- @ExpenseByNature
(1,14,-1,	NULL); 
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,14,	@RawMaterialsAndConsumablesUsed),
(1,0,14,	@CostOfMerchandiseSold),
(2,0,14,	@InsuranceExpense),
(3,0,14,	@ProfessionalFeesExpense),
(4,0,14,	@TransportationExpense),
(5,0,14,	@BankAndSimilarCharges),
(6,0,14,	@TravelExpense),
(7,0,14,	@CommunicationExpense),
(8,0,14,	@UtilitiesExpense),
(9,0,14,	@AdvertisingExpense),
(10,0,14,	@WagesAndSalaries),
(11,0,14,	@SocialSecurityContributions),
(12,0,14,	@OtherShorttermEmployeeBenefits),
--(13,0,14,	@EmployeeBonusExtension),
(14,0,14,	@PostemploymentBenefitExpenseDefinedContributionPlans),
(15,0,14,	@PostemploymentBenefitExpenseDefinedBenefitPlans),
(16,0,14,	@TerminationBenefitsExpense),
(17,0,14,	@OtherLongtermBenefits),
(18,0,14,	@OtherEmployeeExpense),
(19,0,14,	@OtherExpenseByNature),
(0,1,14,	@TradingControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,14,	N'Memo',				0,	N'Memo',			1,2,1),
(1,14,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,14,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,14,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,14,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,14,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
 --21:PaymentFromCustomerCreditSale (inv-gs,cash)
-- credit sale: Dr. Cash, Cr. A/R
-- cash sale: Dr. Cash, Cr. Cash sale Doc control
-- prepayment: Dr. Cash, Cr. VAT Payable, Cr. Unearned Revenues
-- post pay accrual: Dr. Cash, Cr. VAT Payable, Cr. Accrued income
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[MonetaryValue1]	= [MonetaryValue0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1])
'
WHERE [Index] = 21;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,21,+1,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices), -- @CashAndCashEquivalents
(1,21,-1,	NULL); -- @CurrentTradeReceivables
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,21,	@CashAndCashEquivalents),
(0,1,21,	@CurrentTradeReceivables);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,21,	N'Memo',				1,	N'Memo',			1,5,1),
(1,21,	N'ContractId',			1,	N'Customer',		1,4,1),
(2,21,	N'CurrencyId',			0,	N'Currency',		2,0,0),
(3,21,	N'MonetaryValue',		0,	N'Amount',			2,0,0),
(4,21,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(5,21,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--22:PaymentFromCustomerSale (inv-cash-gs) (inv-Cash,gs) (gs,inv-cash)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= [CenterId0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[ContractId2]		= [ContractId1],
		[NotedAmount1]		= ISNULL([MonetaryValue2],0)
'
WHERE [Index] = 22;
INSERT INTO @LineDefinitionEntries([Index],[HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,22,+1,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices), -- @CashAndCashEquivalents
(1,22,-1,	NULL), -- @CurrentValueAddedTaxPayables
(2,22,-1,	NULL); -- @TradingControlExtension
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,22,	@CashAndCashEquivalents),
(0,1,22,	@CurrentValueAddedTaxPayables),
(0,2,22,	@TradingControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,22,	N'Memo',				1,	N'Memo',			1,5,1),
(1,22,	N'NotedContractId',		1,	N'Customer',		1,4,1),
(2,22,	N'CurrencyId',			2,	N'Contract Currency',1,2,1),
(3,22,	N'MonetaryValue',		2,	N'Price Excl. VAT',	1,2,0),
(4,22,	N'MonetaryValue',		1,	N'VAT',				1,2,0),
(5,22,	N'NotedAmount',			0,	N'Total',			2,0,0),
(6,22,	N'NotedDate',			2,	N'Due Date',		3,4,0),
(7,22,	N'NotedDate',			1,	N'Payment Date',	3,5,0),
(8,22,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(9,22,	N'CurrencyId',			0,	N'Rcvd. Currency',	3,4,0),
(10,22,	N'MonetaryValue',		0,	N'Rcvd. Amount',	3,4,0),
(11,22,	N'ExternalReference',	1,	N'Invoice #',		3,5,0),
(22,22,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--33:Service Issue Credit Sale (inv-gs,cash) --31:Stock Issue Credit Sale --32:Stock Issue Sale
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0]	= ISNULL([MonetaryValue1],0) + ISNULL([MonetaryValue2],0),
		[CurrencyId2]		= [CurrencyId0],
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0],
		[NotedContractId2]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 33;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,33,+1), -- @TradingControlExtension
(1,33,-1), -- @CurrentValueAddedTaxPayables
(2,33,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,33,	@TradingControlExtension),
(0,1,33,	@CurrentValueAddedTaxPayables),
(0,2,33,	@RevenueFromRenderingOfServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,33,	N'ContractId',		0,	N'Customer',		1,4,1),
(1,33,	N'CenterId',		2,	N'Profit Center',	1,4,0),
(2,33,	N'ResourceId',		2,	N'Service',			1,4,0),
(3,33,	N'Quantity',		2,	N'Quantity',		1,3,0),
(4,33,	N'UnitId',			2,	N'',				1,3,0),
(5,33,	N'Time1',			2,	N'From',			3,3,1),
(6,33,	N'Time2',			2,	N'Till',			3,3,0),
(7,33,	N'CurrencyId',		0,	N'Currency',		1,4,1),
(8,33,	N'MonetaryValue',	2,	N'Price Excl. VAT',	1,4,0),
(9,33,	N'MonetaryValue',	1,	N'VAT',				1,4,0),
(10,33,	N'MonetaryValue',	0,	N'Price Incl. VAT',	1,0,0);
--(11,33,	N'CenterId',		0,	N'Segment',			4,4,1);
--34:Service Issue Sale,  (inv-cash-gs) (inv-Cash,gs) (gs,inv-cash)
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1]	= [MonetaryValue0],
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 34;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId]) VALUES
(0,34,+1), -- @TradingControlExtension
(1,34,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,34,	@TradingControlExtension),
(0,1,34,	@RevenueFromRenderingOfServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,34,	N'ContractId',		0,	N'Customer',		1,4,1),
(1,34,	N'CenterId',		1,	N'Profit Center',	1,4,0),
(2,34,	N'ResourceId',		1,	N'Service',			1,4,0),
(3,34,	N'Quantity',		1,	N'Quantity',		1,4,0),
(4,34,	N'UnitId',			1,	N'',				1,4,0),
(5,34,	N'Time1',			1,	N'From',			3,4,1),
(6,34,	N'Time2',			1,	N'Till',			3,0,0),
(7,34,	N'CurrencyId',		0,	N'Currency',		1,4,1),
(8,34,	N'MonetaryValue',	0,	N'Price Excl. VAT',	1,4,0);
--(9,34,	N'CenterId',		0,	N'Segment',			4,4,1);
--91:PPEDepreciation
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CenterId1]				= [CenterId0],
		[MonetaryValue0]		= [MonetaryValue1]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 91;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,91,+1,	NULL), -- @DepreciationExpense
(1,91,-1,	@DepreciationPropertyPlantAndEquipment); -- @PropertyPlantAndEquipment
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,91,	@DepreciationExpense),
(1,1,91,	@Buildings),
(2,1,91,	@Machinery),
(3,1,91,	@Vehicles),
(4,1,91,	@FixturesAndFittingsMemberRD),
(5,1,91,	@OfficeEquipment),
(6,1,91,	@TangibleExplorationAndEvaluationAssets),
(7,1,91,	@MiningAssets),
(8,1,91,	@OilAndGasAssets),
--(9,1,91,	@ConstructionInProgress),
(10,1,91,	@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(11,1,91,	@OtherPropertyPlantAndEquipment);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,91,	N'ResourceId',			1,	N'Asset',		1,4,0),
(1,91,	N'Quantity',			1,	N'Usage',		1,4,1),
(2,91,	N'UnitId',				1,	N'',			1,4,1),
(3,91,	N'CenterId',			0,	N'Cost Ctr',	1,4,0),
(4,91,	N'EntryTypeId',			0,	N'Purpose',		1,4,0),
(5,91,	N'Time1',				1,	N'From',		1,4,1),
(6,91,	N'Time2',				1,	N'Till',		1,0,1),
(7,91,	N'MonetaryValue',		1,	N'Depreciation',1,0,0);
*/
DONE_LD:
EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryContractDefinitions = @LineDefinitionEntryContractDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedContractDefinitions = @LineDefinitionEntryNotedContractDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @ProjectCompletionToPropertyPlantAndEquipmentLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToPropertyPlantAndEquipment');
DECLARE @ProjectCompletionToInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToInventory');
DECLARE @ProjectCompletionToInvestmentPropertyLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToInvestmentProperty');
DECLARE @PPEDepreciationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');
DECLARE @IntangibleAmortizationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IntangibleAmortization');
DECLARE @ExchangeVarianceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ExchangeVariance');
DECLARE @TradeSettlementLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'TradeSettlement');
DECLARE @HyperinflationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'Hyperinflation');
DECLARE @CostReallocationToConstructionInProgressLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToConstructionInProgress');
DECLARE @CostReallocationToInvestmentPropertyUnderConstructionOrDevelopmentLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment');
DECLARE @CostReallocationToCurrentInventoriesInTransitLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToCurrentInventoriesInTransit');
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @CashTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransfer');
DECLARE @CashExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashExchange');
DECLARE @DepositCashToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCashToBank');
DECLARE @DepositCheckToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCheckToBank');
DECLARE @CashReceiptFromOtherToCashierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashReceiptFromOtherToCashier');
DECLARE @CheckReceiptFromOtherToCashierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CheckReceiptFromOtherToCashier');
DECLARE @CashPaymentToOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToOther');
DECLARE @CashPaymentToTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToTradePayable');
DECLARE @InvoiceFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InvoiceFromTradePayable');
DECLARE @StockReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptFromTradePayable');
DECLARE @PPEReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEReceiptFromTradePayable');
DECLARE @ConsumableServiceReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptFromTradePayable');
DECLARE @RentalReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RentalReceiptFromTradePayable');
DECLARE @CashPaymentFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentFromTradePayable');
DECLARE @CashReceiptFromTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashReceiptFromTradeReceivable');
DECLARE @CheckReceiptFromTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CheckReceiptFromTradeReceivable');
DECLARE @InvoiceToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InvoiceToTradeReceivable');
DECLARE @StockIssueToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssueToTradeReceivable');
DECLARE @ServiceDeliveryToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceDeliveryToTradeReceivable');

/*
DECLARE @TranslationsLD TABLE (
	[Word] NVARCHAR (50),
	[Lang] NVARCHAR (5),
	PRIMARY KEY ([Word], [Lang]),
	[Translated] NVARCHAR (50)
)
INSERT INTO @TranslationsLD 
([Word],				[Lang], [Translated]) VALUES
(N'Adjustment',			N'ar',	N'تسوية'),
(N'Adjustments',		N'ar',	N'تسويات'),
(N'Other Payment',		N'ar',	N'دفعية أخرى'),
(N'Other Payments',		N'ar',	N'دفعيات أخرى'),
(N'Transfer/Exchange',	N'ar',	N'تحويل\صرف'),		
(N'Transfers/Exchanges',N'ar',	N'تحويلات\صرف'),
(N'Duplicate Line',		N'ar',	N'تسوية'),
(N'Incorrect Analysis', N'ar',	N'تسوية'),
(N'Other reasons',		N'ar',	N'تسوية'),
(N'Payment to Supplier',N'ar',	N'دفعية لمورد'),
(N'Payments to Suppliers',N'ar',N'دفعيات لموردين'),
(N'Memo',				N'ar',	N'البيان'),
(N'Supplier',			N'ar',	N'المورد'),
(N'Employee',			N'ar',	N'الموظف'),
(N'Beneficiary',		N'ar',	N'المستفيد'),
(N'Due Currency',		N'ar',	N'عملة الاستحقاق'),
(N'Total Due',			N'ar',	N'جملة الاستحقاق'),
(N'From Account',		N'ar',	N'من حساب'),
(N'To Account',			N'ar',	N'إلى حساب'),
(N'From Currency',		N'ar',	N'من عملة'),
(N'To Currency',		N'ar',	N'إلى عملة'),
(N'From Amount',		N'ar',	N'من مبلغ'),
(N'To Amount',			N'ar',	N'إلى مبلغ'),
(N'Due Amount',			N'ar',	N'القسط الحالي'),
(N'Pay Currency',		N'ar',	N'عملة الدفع'),
(N'Pay Amount',			N'ar',	N'المبلغ المدفوع'),
(N'Bank/Cashier',		N'ar',	N'البنك\الخزنة'),
(N'Check/Receipt #',	N'ar',	N'رقم الشيك\الإيصال'),
(N'Check Date',			N'ar',	N'تاريخ الشيك'),
(N'Inv. Ctr',			N'ar',	N'مركز الاستثمار');
--(0,9,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
--(1,9,-3,	N'Other reasons',		N'أسباب أخرى');
--N'Payment Employee Benefit',N'دفعية لصالح موظف',	

DECLARE @Lang2 NVARCHAR (5), @Lang3 NVARCHAR (5);
SELECT @Lang2 = SecondaryLanguageId, @Lang3 = TernaryLanguageId FROM dbo.Settings

UPDATE LD
SET LD.[TitleSingular2] = T.[Translated]
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitleSingular] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LD
SET	LD.[TitlePlural2] = T.[Translated]
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitlePlural] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDSR
SET LDSR.[Name2] = T.[Translated]
FROM @LineDefinitionStateReasons LDSR JOIN @TranslationsLD T ON LDSR.[Name] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDC
SET	LDC.[Label2] = T.[Translated]
FROM @LineDefinitionColumns LDC JOIN @TranslationsLD T ON LDC.[Label] = T.[Word] WHERE T.[Lang] = @Lang2
*/
